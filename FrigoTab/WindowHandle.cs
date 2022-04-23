using System;
using System.Runtime.InteropServices;


namespace FrigoTab {
    [Flags]
    public enum WindowStyles : long {
        Disabled = 0x8000000,
        Visible = 0x10000000,
        Minimize = 0x20000000
    }

    [Flags]
    public enum WindowExStyles : long {
        Transparent = 0x20,
        ToolWindow = 0x80,
        AppWindow = 0x40000,
        Layered = 0x80000,
        NoActivate = 0x8000000
    }

    public readonly struct WindowHandle {
        public static readonly WindowHandle Null = new WindowHandle(IntPtr.Zero);
        public static bool operator ==(WindowHandle h1, WindowHandle h2) => h1.Handle == h2.Handle;
        public static bool operator !=(WindowHandle h1, WindowHandle h2) => h1.Handle != h2.Handle;

        public readonly IntPtr Handle;

        public WindowHandle(IntPtr handle) => Handle = handle;
        public override bool Equals(object obj) => obj != null && GetType() == obj.GetType() && Handle == ((WindowHandle) obj).Handle;
        public override int GetHashCode() => Handle.GetHashCode();
        public WindowStyles GetWindowStyles() => (WindowStyles) GetWindowLongPtr(this, WindowLong.Style);
        public WindowExStyles GetWindowExStyles() => (WindowExStyles) GetWindowLongPtr(this, WindowLong.ExStyle);


        private enum WindowLong {
            ExStyle = -20,
            Style = -16
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(WindowHandle hWnd, WindowLong nIndex);
    }
}
