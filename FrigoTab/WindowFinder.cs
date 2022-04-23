using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace FrigoTab {
    public class WindowFinder {
        public readonly IList<WindowHandle> Windows = new List<WindowHandle>();

        public WindowFinder()
        {
            EnumWindows(EnumWindowCallback, IntPtr.Zero);
        }

        private bool EnumWindowCallback(WindowHandle handle, IntPtr lParam)
        {
            var wndType = GetWindowType(handle);
            if (wndType == WindowType.AppWindow) Windows.Add(handle);

            return true;
        }

        private enum WindowType {
            Hidden,
            AppWindow,
            ToolWindow
        }

        private enum WindowAttribute {
            Cloaked = 0xe
        }

        private delegate bool EnumWindowsProc(WindowHandle handle, IntPtr lParam);

        private static WindowType GetWindowType(WindowHandle handle)
        {
            var cloaked = IsCloaked(handle);
            if (cloaked > 0) {
                return WindowType.Hidden;
            }

            var style = handle.GetWindowStyles();
            if (style.HasFlag(WindowStyles.Disabled)) {
                return WindowType.Hidden;
            }

            var visible = style.HasFlag(WindowStyles.Visible);
            if (!visible) {
                return WindowType.Hidden;
            }

            var ex = handle.GetWindowExStyles();
            if (ex.HasFlag(WindowExStyles.NoActivate)) {
                return WindowType.Hidden;
            }

            if (ex.HasFlag(WindowExStyles.AppWindow)) {
                return WindowType.AppWindow;
            }

            if (ex.HasFlag(WindowExStyles.ToolWindow)) {
                return WindowType.ToolWindow;
            }

            return WindowType.AppWindow;
        }

        private static int IsCloaked(WindowHandle window)
        {
            DwmGetWindowAttribute(window, WindowAttribute.Cloaked, out var cloaked, Marshal.SizeOf(typeof(bool)));
            return cloaked;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(WindowHandle hWnd, WindowAttribute dwAttribute, out int pvAttribute, int cbAttribute);
    }
}
