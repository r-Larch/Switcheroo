using System.Drawing;
using System.Runtime.InteropServices;


namespace ManagedWinapi.Windows;

/// <summary>
///     A device context of a window that allows you to draw onto that window.
/// </summary>
public class WindowDeviceContext : IDisposable {
    private readonly SystemWindow _sw;

    internal WindowDeviceContext(SystemWindow sw, IntPtr hDc)
    {
        _sw = sw;
        Hdc = hDc;
    }

    /// <summary>
    ///     The device context handle.
    /// </summary>
    public IntPtr Hdc { get; private set; }

    /// <summary>
    ///     Creates a Graphics object for this device context.
    /// </summary>
    public Graphics CreateGraphics()
    {
        return Graphics.FromHdc(Hdc);
    }

    /// <summary>
    ///     Frees this device context.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (Hdc != IntPtr.Zero) {
            ReleaseDC(_sw.HWnd, Hdc);
            Hdc = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~WindowDeviceContext() => Dispose(false);


    #region PInvoke Declarations

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    #endregion
}
