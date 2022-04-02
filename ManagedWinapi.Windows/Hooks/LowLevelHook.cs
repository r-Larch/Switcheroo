using System.Runtime.InteropServices;


namespace ManagedWinapi.Hooks;

/// <summary>
///     A hook that intercepts keyboard events.
/// </summary>
public class LowLevelKeyboardHook : Hook {
    /// <summary>
    ///     Creates a low-level keyboard hook.
    /// </summary>
    public LowLevelKeyboardHook() : base(HookType.WH_KEYBOARD_LL, true)
    {
        Callback += LowLevelKeyboardHook_Callback;
    }

    /// <summary>
    ///     Called when a key message has been intercepted.
    /// </summary>
    public event LowLevelMessageCallback? MessageIntercepted;

    private int LowLevelKeyboardHook_Callback(int code, IntPtr wParam, IntPtr lParam, ref bool callNext)
    {
        if (code == HC_ACTION) {
            var llh = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT))!;
            var handled = false;

            MessageIntercepted?.Invoke(new LowLevelKeyboardMessage((int) wParam, llh.vkCode, llh.scanCode, llh.flags, llh.time, llh.dwExtraInfo), ref handled);

            if (handled) {
                callNext = false;
                return 1;
            }
        }

        return 0;
    }

    #region PInvoke Declarations

    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct KBDLLHOOKSTRUCT {
        public readonly IntPtr dwExtraInfo;
        public readonly int flags;
        public readonly int scanCode;
        public readonly int time;
        public readonly int vkCode;
    }

    #endregion
}

/// <summary>
///     Represents a method that handles an intercepted low-level message.
/// </summary>
public delegate void LowLevelMessageCallback(LowLevelKeyboardMessage evt, ref bool handled);

/// <summary>
///     A message that has been intercepted by a low-level hook
/// </summary>
public abstract class LowLevelMessage {
    internal LowLevelMessage(int msg, int flags, int time, IntPtr dwExtraInfo)
    {
        Message = msg;
        Flags = flags;
        Time = time;
        ExtraInfo = dwExtraInfo;
    }

    /// <summary>
    ///     The time this message happened.
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    ///     Flags of the message. Its contents depend on the message.
    /// </summary>
    public int Flags { get; }

    /// <summary>
    ///     The message identifier.
    /// </summary>
    public int Message { get; }

    /// <summary>
    ///     Extra information. Its contents depend on the message.
    /// </summary>
    public IntPtr ExtraInfo { get; }
}

/// <summary>
///     A message that has been intercepted by a low-level mouse hook
/// </summary>
public class LowLevelKeyboardMessage : LowLevelMessage {
    /// <summary>
    ///     Creates a new low-level keyboard message.
    /// </summary>
    public LowLevelKeyboardMessage(int msg, int vkCode, int scanCode, int flags, int time, IntPtr dwExtraInfo)
        : base(msg, flags, time, dwExtraInfo)
    {
        VirtualKeyCode = vkCode;
        ScanCode = scanCode;
    }

    /// <summary>
    ///     The virtual key code that caused this message.
    /// </summary>
    public int VirtualKeyCode { get; }

    /// <summary>
    ///     The scan code that caused this message.
    /// </summary>
    public int ScanCode { get; }

    /// <summary>
    ///     Flags needed to replay this event.
    /// </summary>
    public uint KeyboardEventFlags {
        get {
            switch (Message) {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    return 0;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    return KEYEVENTF_KEYUP;
            }

            throw new Exception("Unsupported message");
        }
    }

    #region PInvoke Declarations

    private const int KEYEVENTF_KEYUP = 0x2;
    private const int WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_SYSKEYDOWN = 0x104,
        WM_SYSKEYUP = 0x105;

    #endregion
}
