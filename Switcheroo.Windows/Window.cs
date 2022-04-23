using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.WindowsAndMessaging;


namespace Switcheroo.Windows;

public struct Window : IEquatable<Window> {
    public HWnd HWnd { get; }
    public Window(HWnd hWnd) => HWnd = hWnd;
    private string? _className = null;


    public unsafe string Title {
        get {
            var size = PInvoke.GetWindowTextLength(HWnd) + 1;
            if (size <= 64) {
                var text = stackalloc char[size];
                var length = PInvoke.GetWindowText(HWnd, lpString: text, nMaxCount: size);
                return new string(text, 0, length);
            }

            fixed (char* text = new char[size]) {
                var length = PInvoke.GetWindowText(HWnd, lpString: text, nMaxCount: size);
                return new string(text, 0, length);
            }
        }
    }


    public Window Owner {
        get {
            var hwnd = PInvoke.GetWindow(HWnd, GET_WINDOW_CMD.GW_OWNER);
            return new Window(hwnd);
        }
    }


    public unsafe uint ProcessId {
        get {
            uint pid = 0;
            PInvoke.GetWindowThreadProcessId(HWnd, &pid);
            return pid;
        }
    }


    public bool Visible => PInvoke.IsWindowVisible(HWnd);


    /// <summary>
    /// Whether this window is currently enabled (able to accept keyboard input).
    /// </summary>
    public bool Enabled => PInvoke.IsWindowEnabled(HWnd);


    /// <summary>
    /// The name of the window class (by the <c>GetClassName</c> API function).
    /// This class has nothing to do with classes in C# or other .NET languages.
    /// </summary>
    public unsafe string ClassName {
        get {
            if (_className == null) {
                var length = 64;
                while (true) {
#pragma warning disable CA2014 // Do not use stackalloc in loops
                    fixed (char* className = length == 64 ? stackalloc char[length] : new char[length]) {
                        var count = PInvoke.GetClassName(HWnd, className, length);
                        if (count == 0) {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        if (count < length - 1) {
                            _className = new string(className, 0, count);
                            break;
                        }

                        length *= 2;
                    }
#pragma warning restore CA2014 // Do not use stackalloc in loops
                }
            }

            return _className;
        }
    }


    public WindowStyles WindowStyles => (WindowStyles) PInvoke.GetWindowLongPtr(HWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
    public WindowStylesEx WindowExStyles => (WindowStylesEx) PInvoke.GetWindowLongPtr(HWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);


    public unsafe bool IsCloaked {
        get {
            int cloaked;
            PInvoke.DwmGetWindowAttribute(HWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, pvAttribute: &cloaked, sizeof(int));

            //const int DWM_CLOAKED_APP = 0x0000001; // The window was cloaked by its owner application.
            //const int DWM_CLOAKED_SHELL = 0x0000002; // The window was cloaked by the Shell.
            //const int DWM_CLOAKED_INHERITED = 0x0000004; // The cloak value was inherited from its owner window.
            return cloaked > 0;
        }
    }


    public unsafe bool IsClosed {
        get {
            // the window is closed if GetClassName fails
            var name = stackalloc char[2];
            return PInvoke.GetClassName(HWnd, name, 2) == 0;
        }
    }

    public bool IsClosedOrHidden => IsClosed || !Visible;


    public void SwitchToLastVisibleActivePopup()
    {
        var lastActiveVisiblePopup = GetLastActiveVisiblePopup();
        PInvoke.SwitchToThisWindow(lastActiveVisiblePopup, true);
    }


    private IntPtr GetLastActiveVisiblePopup()
    {
        // Which windows appear in the Alt+Tab list? -Raymond Chen
        // http://blogs.msdn.com/b/oldnewthing/archive/2007/10/08/5351207.aspx

        // Start at the root owner
        var hwndWalk = PInvoke.GetAncestor(HWnd, GET_ANCESTOR_FLAGS.GA_ROOTOWNER);

        // See if we are the last active visible popup
        var hwndTry = (HWnd) 0;
        while (hwndWalk != hwndTry) {
            hwndTry = hwndWalk;
            hwndWalk = PInvoke.GetLastActivePopup(hwndTry);
            if (PInvoke.IsWindowVisible(hwndWalk)) return hwndWalk;
        }

        return hwndWalk;
    }


    private const uint WM_CLOSE = 0x10, WM_SYSCOMMAND = 0x112;
    private const nint SC_CLOSE = 0xF060;

    /// <summary>
    /// Send a message to this window that it should close. This is equivalent
    /// to clicking the "X" in the upper right corner or pressing Alt+F4.
    /// </summary>
    public void SendClose()
    {
        PInvoke.SendMessage(HWnd, WM_CLOSE, 0, 0);
    }

    /// <summary>
    /// Post a message to this window that it should close. This is equivalent
    /// to clicking the "X" in the upper right corner or pressing Alt+F4.
    /// It sometimes works in instances where the <see cref="SendClose" /> function does
    /// not (for example, Windows Explorer windows.)
    /// </summary>
    public void PostClose()
    {
        PInvoke.PostMessage(HWnd, WM_CLOSE, 0, 0);
    }

    /// <summary>
    /// Closes the window by sending the "WM_SYSCOMMAND" with the "SC_CLOSE" parameter.
    /// This equals that the user open the Window menu and click "Close". This method
    /// seem to work in more scenaries than "SendClose()" and "PostClose()".
    /// Also see: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
    /// </summary>
    public void Close()
    {
        PInvoke.PostMessage(HWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
    }


    public string ProcessTitle {
        get {
            string processTitle;

            if (IsUwpApplication()) {
                processTitle = "UWP";
                var processId = ProcessId;
                var underlyingProcessWindow = DirectChildWindows().FirstOrDefault(w => w.ProcessId != processId);
                if (underlyingProcessWindow.HWnd != 0 && underlyingProcessWindow.ProcessName != "") processTitle = underlyingProcessWindow.ProcessName;
            }
            else {
                processTitle = ProcessName;
            }

            return processTitle;
        }
    }


    private unsafe IReadOnlyCollection<Window> DirectChildWindows()
    {
        var context = (list: new List<Window>(), hWnd: HWnd);
        PInvoke.EnumChildWindows(HWnd, &LpEnumFunc, (nint) Unsafe.AsPointer(ref context));
        return context.list;

        [UnmanagedCallersOnly]
        static BOOL LpEnumFunc(HWnd hwnd, nint lParam)
        {
            var (list, hWnd) = Unsafe.Read<(List<Window> List, HWnd HWnd)>((void*) lParam);
            if (PInvoke.GetParent(hwnd) == hWnd) list.Add(new Window(hwnd));
            return true;
        }
    }


    /// <summary>
    /// The name of the process which created this window.
    /// </summary>
    public unsafe string ProcessName {
        get {
            var hProcess = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION /*| PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ*/, false, ProcessId);
            if (hProcess == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try {
                uint exeBufferSize = 512;
                fixed (char* exeBuffer = new char[exeBufferSize]) {
                    if (!PInvoke.QueryFullProcessImageName(hProcess, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, exeBuffer, &exeBufferSize)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    var path = new string(exeBuffer, 0, (int) exeBufferSize);
                    return ProcessNameRegex.Match(path).Value;
                }
            }
            finally {
                PInvoke.CloseHandle(hProcess);
            }
        }
    }


    private static readonly Regex ProcessNameRegex = new("([^\\\\]+?)(?=(\\.exe)?$)");


    public bool IsUwpApplication()
    {
        // Is a UWP application
        return ClassName == "ApplicationFrameWindow";
    }


    public bool IsCoreWindow()
    {
        // Avoids double entries for Windows Store Apps on Windows 10
        return ClassName == "Windows.UI.Core.CoreWindow";
    }


    public unsafe string ExecutablePath {
        get {
            var hProcess = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessId);
            if (hProcess == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try {
                uint size = 1024;
                fixed (char* buffer = new char[size]) {
                    if (PInvoke.QueryFullProcessImageName(hProcess, PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, buffer, &size)) {
                        return new string(buffer, 0, (int) size);
                    }
                }
            }
            finally {
                PInvoke.CloseHandle(hProcess);
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }


    public Icon? LargeWindowIcon => new WindowIconFinder().Find(this, WindowIconSize.Large);

    public Icon? SmallWindowIcon => new WindowIconFinder().Find(this, WindowIconSize.Small);


    #region Equality

    public bool Equals(Window other) => HWnd.Equals(other.HWnd);
    public override bool Equals(object? obj) => obj is Window other && Equals(other);
    public override int GetHashCode() => HWnd.GetHashCode();
    public static implicit operator HWnd(Window window) => window.HWnd;
    public static implicit operator nint(Window window) => window.HWnd;
    public static bool operator ==(Window h1, Window h2) => h1.HWnd == h2.HWnd;
    public static bool operator !=(Window h1, Window h2) => h1.HWnd != h2.HWnd;
    public override string ToString() => $"Window({HWnd})".ToString();

    #endregion
}
