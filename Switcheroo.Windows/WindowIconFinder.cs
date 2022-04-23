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
        public unsafe Icon? Find(AppWindow appWindow, WindowIconSize size)
        {
            Icon? icon = null;
            try {
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632625(v=vs.85).aspx
                nuint response;
                var result = PInvoke.SendMessageTimeout(
                    appWindow.HWnd,
                    Msg: 0x007F,
                    wParam: (nuint) (size == WindowIconSize.Small ? 2 : 1),
                    lParam: IntPtr.Zero,
                    fuFlags: SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
                    uTimeout: 100,
                    lpdwResult: &response
                );

                if (result == IntPtr.Zero || response == 0) {
                    response = (nuint) PInvoke.GetClassLongPtr(
                        appWindow.HWnd,
                        size == WindowIconSize.Small
                            ? WNDCLASSEXA_INDEX.GCLP_HICONSM
                            : WNDCLASSEXA_INDEX.GCLP_HICON
                    );
                }

                if (response != 0) {
                    icon = Icon.FromHandle((nint) response);
                }
                else {
                    var executablePath = appWindow.ExecutablePath;
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
