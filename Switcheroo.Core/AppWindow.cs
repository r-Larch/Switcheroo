/*
 * Switcheroo - The incremental-search task switcher for Windows.
 * http://www.switcheroo.io/
 * Copyright 2009, 2010 James Sulak
 * Copyright 2014 Regin Larsen
 *
 * Switcheroo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Switcheroo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Switcheroo.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Text;
using ManagedWinapi.Windows;


namespace Switcheroo.Core;

/// <summary>
///     This class is a wrapper around the Win32 api window handles
/// </summary>
public class AppWindow1 : SystemWindow {
    public AppWindow1(IntPtr HWnd) : base(HWnd)
    {
    }

    public bool IsForegroundWindow { get; set; }

    public string ProcessTitle {
        get {
            var key = "ProcessTitle-" + HWnd;
            var processTitle = MemoryCache.Default.Get(key) as string;
            if (processTitle == null) {
                if (IsApplicationFrameWindow()) {
                    processTitle = "UWP";
                    var underlyingProcessWindow = AllChildWindows.FirstOrDefault(w => w.ProcessId != ProcessId);
                    if (underlyingProcessWindow != null && underlyingProcessWindow.ProcessName != "") processTitle = underlyingProcessWindow.ProcessName;
                }
                else {
                    processTitle = ProcessName;
                }

                MemoryCache.Default.Add(key, processTitle, DateTimeOffset.Now.AddHours(1));
            }

            return processTitle;
        }
    }

    public Icon LargeWindowIcon => new WindowIconFinder().Find(this, WindowIconSize.Large);

    public Icon SmallWindowIcon => new WindowIconFinder().Find(this, WindowIconSize.Small);

    public string ExecutablePath => GetExecutablePath(Process.Id);

    public new AppWindow1 Owner {
        get {
            var ownerHandle = WinApi.GetWindow(HWnd, WinApi.GetWindowCmd.GW_OWNER);
            if (ownerHandle == IntPtr.Zero) return null;
            return new AppWindow1(ownerHandle);
        }
    }

    public new static IEnumerable<AppWindow1> AllTopLevelWindows {
        get {
            return SystemWindow.AllTopLevelWindows
                .Select(w => new AppWindow1(w.HWnd));
        }
    }

    /// <summary>
    ///     Sets the focus to this window and brings it to the foreground.
    /// </summary>
    public void SwitchTo()
    {
        // This function is deprecated, so should probably be replaced.
        WinApi.SwitchToThisWindow(HWnd, true);
    }

    public void SwitchToLastVisibleActivePopup()
    {
        var lastActiveVisiblePopup = GetLastActiveVisiblePopup();
        WinApi.SwitchToThisWindow(lastActiveVisiblePopup, true);
    }

    public bool IsAltTabWindow()
    {
        if (
            Visible &&
            !string.IsNullOrEmpty(Title) &&
            (
                IsAppWindow() ||
                IsToolWindow() ||
                (
                    !IsNoActivate() &&
                    IsOwnerOrOwnerNotVisible() &&
                    !HasITaskListDeletedProperty() &&
                    !IsCoreWindow()
                )
            ) &&
            (
                !IsApplicationFrameWindow() ||
                HasAppropriateApplicationViewCloakType()
            )
        ) return true;

        return false;
    }


    private bool IsToolWindow()
    {
        return (ExtendedStyle & WindowExStyleFlags.TOOLWINDOW) == WindowExStyleFlags.TOOLWINDOW
               || (Style & WindowStyleFlags.TOOLWINDOW) == WindowStyleFlags.TOOLWINDOW;
    }

    private bool IsAppWindow()
    {
        return (ExtendedStyle & WindowExStyleFlags.APPWINDOW) == WindowExStyleFlags.APPWINDOW;
    }

    private bool IsNoActivate()
    {
        return (ExtendedStyle & WindowExStyleFlags.NOACTIVATE) == WindowExStyleFlags.NOACTIVATE;
    }

    private IntPtr GetLastActiveVisiblePopup()
    {
        // Which windows appear in the Alt+Tab list? -Raymond Chen
        // http://blogs.msdn.com/b/oldnewthing/archive/2007/10/08/5351207.aspx

        // Start at the root owner
        var hwndWalk = WinApi.GetAncestor(HWnd, WinApi.GetAncestorFlags.GetRootOwner);

        // See if we are the last active visible popup
        var hwndTry = IntPtr.Zero;
        while (hwndWalk != hwndTry) {
            hwndTry = hwndWalk;
            hwndWalk = WinApi.GetLastActivePopup(hwndTry);
            if (WinApi.IsWindowVisible(hwndWalk)) return hwndWalk;
        }

        return hwndWalk;
    }

    private bool IsOwnerOrOwnerNotVisible()
    {
        return Owner == null || !(Owner.Visible && Owner.Enabled);
    }

    private bool HasITaskListDeletedProperty()
    {
        return WinApi.GetProp(HWnd, "ITaskList_Deleted") != IntPtr.Zero;
    }

    private bool IsCoreWindow()
    {
        // Avoids double entries for Windows Store Apps on Windows 10
        return ClassName == "Windows.UI.Core.CoreWindow";
    }

    private bool IsApplicationFrameWindow()
    {
        // Is a UWP application
        return ClassName == "ApplicationFrameWindow";
    }

    private bool HasAppropriateApplicationViewCloakType()
    {
        // The ApplicationFrameWindows that host Windows Store Apps like to
        // hang around in Windows 10 even after the underlying program has been
        // closed. A way to figure out if the ApplicationFrameWindow is
        // currently hosting an application is to check if it has a property called
        // "ApplicationViewCloakType", and that the value != 1.
        //
        // I've stumbled upon these values of "ApplicationViewCloakType":
        //    0 = Program is running on current virtual desktop
        //    1 = Program is not running
        //    2 = Program is running on a different virtual desktop

        var hasAppropriateApplicationViewCloakType = false;
        WinApi.EnumPropsEx(HWnd, (hwnd, lpszString, data, dwData) => {
            var propName = Marshal.PtrToStringAnsi(lpszString);
            if (propName == "ApplicationViewCloakType") {
                hasAppropriateApplicationViewCloakType = data != 1;
                return 0;
            }

            return 1;
        }, IntPtr.Zero);

        return hasAppropriateApplicationViewCloakType;
    }

    // This method only works on Windows >= Windows Vista
    private static string GetExecutablePath(int processId)
    {
        var buffer = new StringBuilder(1024);
        var hprocess = WinApi.OpenProcess(WinApi.ProcessAccess.QueryLimitedInformation, false, processId);
        if (hprocess == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());

        try {
            // ReSharper disable once RedundantAssignment
            var size = buffer.Capacity;
            if (WinApi.QueryFullProcessImageName(hprocess, 0, buffer, out size)) return buffer.ToString();
        }
        finally {
            WinApi.CloseHandle(hprocess);
        }

        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}
