using System.ComponentModel;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;


namespace Switcheroo.Windows {
    public enum WindowIconSize {
        Small,
        Large
    }

    public struct WindowIconFinder {
        public unsafe Icon? Find(Window window, WindowIconSize size)
        {
            Icon? icon = null;
            try {
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632625(v=vs.85).aspx
                nuint response;
                var result = PInvoke.SendMessageTimeout(
                    window.HWnd,
                    Msg: 0x007F,
                    wParam: (nuint) (size == WindowIconSize.Small ? 2 : 1),
                    lParam: IntPtr.Zero,
                    fuFlags: SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
                    uTimeout: 100,
                    lpdwResult: &response
                );

                if (result == IntPtr.Zero || response == 0) {
                    response = PInvoke.GetClassLongPtr(
                        window.HWnd,
                        size == WindowIconSize.Small
                            ? WinApi.ClassLongFlags.GCLP_HICONSM
                            : WinApi.ClassLongFlags.GCLP_HICON
                    );
                }

                if (response != 0) {
                    icon = Icon.FromHandle((nint) response);
                }
                else {
                    var executablePath = window.ExecutablePath;
                    icon = Icon.ExtractAssociatedIcon(executablePath);
                }
            }
            catch (Win32Exception) {
                // Could not extract icon
            }

            return icon;
        }
    }
}
