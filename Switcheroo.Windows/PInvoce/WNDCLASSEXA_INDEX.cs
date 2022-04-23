// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace Windows.Win32;

public enum WNDCLASSEXA_INDEX : int {
    /// Retrieves an ATOM value that uniquely identifies the window class. This is the same atom that the RegisterClassEx function returns.
    GCW_ATOM = -32,
    /// Retrieves the size, in bytes, of the extra memory associated with the class.
    GCL_CBCLSEXTRA = -20,
    /// Retrieves the size, in bytes, of the extra window memory associated with each window in the class. For information on how to access this memory, see GetWindowLongPtr.
    GCL_CBWNDEXTRA = -18,
    /// Retrieves a handle to the background brush associated with the class.
    GCLP_HBRBACKGROUND = -10,
    /// Retrieves a handle to the cursor associated with the class.
    GCLP_HCURSOR = -12,
    /// Retrieves a handle to the icon associated with the class.
    GCLP_HICON = -14,
    /// Retrieves a handle to the small icon associated with the class.
    GCLP_HICONSM = -34,
    /// Retrieves a handle to the module that registered the class.
    GCLP_HMODULE = -16,
    /// Retrieves the pointer to the menu name string. The string identifies the menu resource associated with the class.
    GCLP_MENUNAME = -8,
    /// Retrieves the window-class style bits.
    GCL_STYLE = -26,
    /// Retrieves the address of the window procedure, or a handle representing the address of the window procedure. You must use the CallWindowProc function to call the window procedure.
    GCLP_WNDPROC = -24,
}
