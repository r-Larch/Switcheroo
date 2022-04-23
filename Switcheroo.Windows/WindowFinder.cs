using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;


namespace Switcheroo.Windows;

public class WindowFinder {
    public AppWindow ForegroundAppWindow;
    public bool IsWindowsNativeTaskSwitcherActive => ForegroundAppWindow.ClassName == "MultitaskingViewFrame";
    public bool IsForegroundWindow(AppWindow hWnd) => hWnd == ForegroundAppWindow || ForegroundAppWindow.ProcessId == hWnd.ProcessId;
    public void SaveForegroundWindow() => ForegroundAppWindow = new AppWindow(GetForegroundWindow());

    public void SetForegroundWindow(HWnd hWnd)
    {
        if (PInvoke.SetForegroundWindow(hWnd) == false) throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public unsafe IReadOnlyCollection<AppWindow> GetWindows()
    {
        var windows = new WindowCollection();
        PInvoke.EnumWindows(&LpEnumFunc, (nint) Unsafe.AsPointer(ref windows));
        return windows;

        [UnmanagedCallersOnly]
        static BOOL LpEnumFunc(HWnd hwnd, nint lParam)
        {
            var window = new AppWindow(hwnd);
            if (IsAltTapWindow(window)) {
                Unsafe.Read<WindowCollection>((void*) lParam).Add(window);
            }

            return true;
        }
    }


    #region private

    private static HWnd GetForegroundWindow() => PInvoke.GetForegroundWindow();

    private static bool IsAltTapWindow(AppWindow appWindow)
    {
        if (appWindow.IsCloaked) {
            return false;
        }

        var style = appWindow.WindowStyles;

        var isDisabled = (style & WindowStyles.WS_DISABLED) == WindowStyles.WS_DISABLED;
        var isVisible = (style & WindowStyles.WS_VISIBLE) == WindowStyles.WS_VISIBLE;

        if (isDisabled || !isVisible) {
            return false;
        }

        var exStyle = appWindow.WindowExStyles;

        var isNoActivate = (exStyle & WindowStylesEx.WS_EX_NOACTIVATE) == WindowStylesEx.WS_EX_NOACTIVATE;
        var isAppWindow = (exStyle & WindowStylesEx.WS_EX_APPWINDOW) == WindowStylesEx.WS_EX_APPWINDOW;
        var isToolWindow = (
            (exStyle & WindowStylesEx.WS_EX_TOOLWINDOW) == WindowStylesEx.WS_EX_TOOLWINDOW ||
            // search for the exStyle in window style because it seems some windows specify it wrong
            ((WindowStylesEx) style & WindowStylesEx.WS_EX_TOOLWINDOW) == WindowStylesEx.WS_EX_TOOLWINDOW
        );

        if (isNoActivate || !isAppWindow && isToolWindow) {
            return false;
        }

        if (!appWindow.Visible) return false;
        if (string.IsNullOrEmpty(appWindow.Title)) return false;

        if (
            !isAppWindow &&
            !isToolWindow &&
            (
                isNoActivate ||
                !IsOwnerOrOwnerNotVisible(appWindow) ||
                IsSlackNotification(appWindow) ||
                appWindow.IsCoreWindow()
            )
        ) return false;

        if (
            appWindow.IsUwpApplication() &&
            !HasApplicationViewCloakTypeRunning(appWindow)
        ) return false;

        return true;
    }

    private static bool IsOwnerOrOwnerNotVisible(AppWindow hwnd)
    {
        var owner = hwnd.Owner;
        return owner.HWnd == 0 || !(owner.Visible && owner.Enabled);
    }

    private static unsafe bool IsSlackNotification(HWnd hwnd)
    {
        // Windows with property ITaskList_Deleted should be excluded (https://github.com/kvakulo/Switcheroo/commit/2199c26df60f511d966fc4c2adaaec2a99b7a2fd)
        // E.g.Slack notifications have this property.
        fixed (char* lpString = "ITaskList_Deleted")
            return PInvoke.GetProp(hwnd, lpString) != 0;
    }

    private static unsafe bool HasApplicationViewCloakTypeRunning(HWnd hwnd)
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

        fixed (char* lpString = "ApplicationViewCloakType") {
            var isRunning = PInvoke.GetProp(hwnd, lpString) != 1;
            return isRunning;
        }
    }

    #endregion
}
