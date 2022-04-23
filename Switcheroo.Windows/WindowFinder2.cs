using Windows.Win32;


namespace Switcheroo.Windows;

public class WindowFinder2 {
    public IEnumerable<WindowHandle> GetWindows()
    {
        foreach (var window in WindowHandle.GetAllWindows()) {
            // TODO add filter..

            var isCloaked = window.IsCloaked();
            if (isCloaked) {
                continue;
            }

            var style = window.GetWindowStyles();

            var isDisabled = (style & WindowStyles.WS_DISABLED) == WindowStyles.WS_DISABLED;
            var isVisible = (style & WindowStyles.WS_VISIBLE) == WindowStyles.WS_VISIBLE;

            if (isDisabled || !isVisible) {
                continue;
            }

            var exStyle = window.GetWindowExStyles();

            var isNoActivate = (exStyle & WindowStylesEx.WS_EX_NOACTIVATE) == WindowStylesEx.WS_EX_NOACTIVATE;
            var isAppWindow = (exStyle & WindowStylesEx.WS_EX_APPWINDOW) == WindowStylesEx.WS_EX_APPWINDOW;
            var isToolWindow = (
                (exStyle & WindowStylesEx.WS_EX_TOOLWINDOW) == WindowStylesEx.WS_EX_TOOLWINDOW //||
                // search for the exStyle in window style because it seems some windows specify it wrong
                // TODO ((WindowStylesEx) style & WindowStylesEx.WS_EX_TOOLWINDOW) == WindowStylesEx.WS_EX_TOOLWINDOW;
            );

            if (isNoActivate || !isAppWindow && isToolWindow) {
                continue;
            }

            var className = window.ClassName;

            if (
                window.Visible &&
                !string.IsNullOrEmpty(window.Title) &&
                (
                    isAppWindow ||
                    isToolWindow /*plus extra check*/ ||
                    (
                        !isNoActivate &&
                        IsOwnerOrOwnerNotVisible(window) &&
                        !HasITaskListDeletedProperty(window) &&
                        !IsCoreWindow(className)
                    )
                ) &&
                (
                    !IsApplicationFrameWindow(className) ||
                    HasAppropriateApplicationViewCloakType(window)
                )
            ) {
                yield return window;
            }
        }
    }


    private static bool IsCoreWindow(string windowClassName)
    {
        // Avoids double entries for Windows Store Apps on Windows 10
        return windowClassName == "Windows.UI.Core.CoreWindow";
    }

    private static bool IsOwnerOrOwnerNotVisible(WindowHandle window)
    {
        return window.Owner == WindowHandle.Null || !(window.Owner.Visible && window.Owner.Enabled);
    }

    private static unsafe bool HasITaskListDeletedProperty(WindowHandle window)
    {
        fixed (char* lpString = "ITaskList_Deleted")
            return PInvoke.GetProp(window.HWnd, lpString) != IntPtr.Zero;
    }


    private static bool IsApplicationFrameWindow(string windowClassName)
    {
        // Is a UWP application
        return windowClassName == "ApplicationFrameWindow";
    }

    private unsafe bool HasAppropriateApplicationViewCloakType(WindowHandle window)
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
            var isRunning = (nint) PInvoke.GetProp(window.HWnd, lpString) != 1;
            return isRunning;
        }
    }
}
