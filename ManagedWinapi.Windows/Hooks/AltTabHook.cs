using System.Runtime.InteropServices;


namespace ManagedWinapi.Hooks;

public class AltTabHook : IDisposable {
    private readonly LowLevelKeyboardHook _lowLevelKeyboardHook;

    private readonly int WM_KEYDOWN = 0x0100;
    private readonly int WM_SYSKEYDOWN = 0x0104;

    private enum Keys : ushort {
        Alt = VK_LMENU,
        Ctrl = VK_LCONTROL,
        Shift = VK_LSHIFT,
        Tab = VK_TAB,

        // WinApi Virtual-Key Codes:
        VK_LMENU = 0xA4,
        VK_LCONTROL = 0xA2,
        VK_LSHIFT = 0xA0,
        VK_TAB = 0x09,
    }

    public AltTabHook()
    {
        _lowLevelKeyboardHook = new LowLevelKeyboardHook();
        _lowLevelKeyboardHook.MessageIntercepted += OnMessageIntercepted;
        _lowLevelKeyboardHook.StartHook();
    }

    public event AltTabHookEventHandler? Pressed;

    private void OnMessageIntercepted(LowLevelKeyboardMessage keyboardMessage, ref bool handled)
    {
        if (handled) {
            return;
        }

        if (!IsTabKeyDown(keyboardMessage)) {
            return;
        }

        if (!IsAltKeyDown(keyboardMessage)) {
            return;
        }

        var shiftKeyDown = IsKeyDown(Keys.Shift);
        var ctrlKeyDown = IsKeyDown(Keys.Ctrl);

        var eventArgs = OnPressed(shiftKeyDown, ctrlKeyDown);

        handled = eventArgs.Handled;
    }

    private static bool IsKeyDown(Keys key)
    {
        var asyncState = GetAsyncKeyState((ushort) key);
        return (asyncState & 0x8000) != 0;
    }

    private bool IsTabKeyDown(LowLevelKeyboardMessage keyboardMessage)
    {
        return keyboardMessage.VirtualKeyCode == (int) Keys.Tab &&
               (keyboardMessage.Message == WM_KEYDOWN || keyboardMessage.Message == WM_SYSKEYDOWN);
    }

    private static bool IsAltKeyDown(LowLevelKeyboardMessage keyboardMessage)
    {
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-kbdllhookstruct
        // 0x20: the 6. bit - The context code. The value is 1 if the ALT key is pressed; otherwise, it is 0.
        return (keyboardMessage.Flags & 0x20) == 0x20;
    }

    private AltTabHookEventArgs OnPressed(bool shiftDown, bool ctrlDown)
    {
        var altTabHookEventArgs = new AltTabHookEventArgs { ShiftDown = shiftDown, CtrlDown = ctrlDown };
        Pressed?.Invoke(this, altTabHookEventArgs);

        return altTabHookEventArgs;
    }


    protected virtual void Dispose(bool disposing)
    {
        _lowLevelKeyboardHook.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AltTabHook() => Dispose();


    #region PInvoke Declarations

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    #endregion
}

public delegate void AltTabHookEventHandler(object sender, AltTabHookEventArgs args);

public class AltTabHookEventArgs : EventArgs {
    public bool CtrlDown { get; set; }
    public bool ShiftDown { get; set; }
    public bool Handled { get; set; }
}
