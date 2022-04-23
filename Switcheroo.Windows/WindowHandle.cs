using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;


namespace Switcheroo.Windows;

public readonly struct WindowHandle : IEquatable<WindowHandle> {
    public static WindowHandle Null = new(new HWND(0));
    internal WindowHandle(HWND hWnd) => HWnd = hWnd;
    internal readonly HWND HWnd;


    public static IEnumerable<WindowHandle> GetAllWindows()
    {
        var windows = new List<WindowHandle>(20);

        PInvoke.EnumWindows(delegate(HWND hwnd, LPARAM lParam) {
            windows.Add(new WindowHandle(hwnd));
            return true;
        }, IntPtr.Zero);
        return windows;
    }


    public bool Visible => PInvoke.IsWindowVisible(HWnd);

    /// <summary>
    /// Whether this window is currently enabled (able to accept keyboard input).
    /// </summary>
    public bool Enabled {
        get => PInvoke.IsWindowEnabled(HWnd);
        set => PInvoke.EnableWindow(HWnd, value);
    }

    public string? Title {
        get {
            unsafe {
                var size = PInvoke.GetWindowTextLength(HWnd) + 1;
                var text = stackalloc char[size];
                var length = PInvoke.GetWindowText(HWnd, lpString: text, nMaxCount: size);
                return new string(text, 0, length);
            }
        }

        set => PInvoke.SetWindowText(HWnd, value);
    }

    public WindowHandle Owner {
        get {
            var hwnd = PInvoke.GetWindow(HWnd, GET_WINDOW_CMD.GW_OWNER);
            return new WindowHandle(hwnd);
        }
    }

    /// <summary>
    /// The name of the window class (by the <c>GetClassName</c> API function).
    /// This class has nothing to do with classes in C# or other .NET languages.
    /// </summary>
    public unsafe string ClassName {
        get {
            var length = 64;
            while (true) {
                fixed (char* className = new char[length]) {
                    var count = PInvoke.GetClassName(HWnd, className, length);
                    if (count == 0) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (count < length - 1) {
                        return new string(className, 0, count);
                    }

                    length *= 2;
                }
            }
        }
    }

    public WindowStyles GetWindowStyles() => (WindowStyles) PInvoke.GetWindowLongPtr(HWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
    public WindowStylesEx GetWindowExStyles() => (WindowStylesEx) PInvoke.GetWindowLongPtr(HWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);


    public bool IsCloaked()
    {
        unsafe {
            int cloaked;
            PInvoke.DwmGetWindowAttribute(HWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, pvAttribute: &cloaked, sizeof(int));

            //const int DWM_CLOAKED_APP = 0x0000001; // The window was cloaked by its owner application.
            //const int DWM_CLOAKED_SHELL = 0x0000002; // The window was cloaked by the Shell.
            //const int DWM_CLOAKED_INHERITED = 0x0000004; // The cloak value was inherited from its owner window.
            return cloaked > 0;
        }
    }


    #region Equality

    public bool Equals(WindowHandle other) => HWnd.Equals(other.HWnd);
    public override bool Equals(object? obj) => obj is WindowHandle other && Equals(other);
    public override int GetHashCode() => HWnd.GetHashCode();
    public static bool operator ==(WindowHandle h1, WindowHandle h2) => h1.HWnd == h2.HWnd;
    public static bool operator !=(WindowHandle h1, WindowHandle h2) => h1.HWnd != h2.HWnd;

    #endregion
}
