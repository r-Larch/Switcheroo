using System;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace ManagedWinapi.Hooks;

/// <summary>
///     A hook is a point in the system message-handling mechanism where an application
///     can install a subroutine to monitor the message traffic in the system and process
///     certain types of messages before they reach the target window procedure.
/// </summary>
public class Hook : IDisposable {
    /// <summary>
    ///     Represents a method that handles a callback from a hook.
    /// </summary>
    public delegate int HookCallback(int code, IntPtr wParam, IntPtr lParam, ref bool callNext);

    private readonly HookProc _managedDelegate;
    private IntPtr _hHook;
    private readonly bool _global;

    /// <summary>
    ///     Creates a new hook.
    /// </summary>
    public Hook(HookType type, bool global)
    {
        Type = type;
        _global = global;
        _managedDelegate = InternalCallback;
    }

    /// <summary>
    ///     The type of the hook.
    /// </summary>
    public HookType Type { get; set; }

    /// <summary>
    ///     Whether this hook has been started.
    /// </summary>
    public bool Hooked { get; private set; }

    /// <summary>
    ///     Occurs when the hook's callback is called.
    /// </summary>
    public event HookCallback? Callback;

    /// <summary>
    ///     Hooks the hook.
    /// </summary>
    public virtual void StartHook()
    {
        if (Hooked) return;
        var func = Marshal.GetFunctionPointerForDelegate(_managedDelegate);
        if (_global) {
            // http://stackoverflow.com/a/17898148/198065
            var moduleHandle = LoadLibrary("user32.dll");
            _hHook = SetWindowsHookEx(Type, func, moduleHandle, 0);
        }
        else {
            _hHook = SetWindowsHookEx(Type, func, IntPtr.Zero, GetCurrentThreadId());
        }

        if (_hHook == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        Hooked = true;
    }


    /// <summary>
    ///     Unhooks the hook.
    /// </summary>
    public virtual void Unhook()
    {
        if (Hooked) {
            if (!UnhookWindowsHookEx(_hHook)) throw new Win32Exception(Marshal.GetLastWin32Error());
            Hooked = false;
        }
    }


    /// <summary>
    ///     Override this method if you want to prevent a call
    ///     to the CallNextHookEx method or if you want to return
    ///     a different return value. For most hooks this is not needed.
    /// </summary>
    protected virtual int InternalCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && Callback != null) {
            var callNext = true;
            var result = Callback(code, wParam, lParam, ref callNext);
            if (!callNext) return result;
        }

        return CallNextHookEx(_hHook, code, wParam, lParam);
    }

    /// <summary>
    ///     Unhooks the hook if necessary.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (Hooked) {
            Unhook();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Hook() => Dispose(false);


    #region PInvoke Declarations

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(HookType hook, IntPtr callback, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    internal static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("ManagedWinapiNativeHelper.dll")]
    private static extern IntPtr AllocHookWrapper(IntPtr callback);

    [DllImport("ManagedWinapiNativeHelper.dll")]
    private static extern bool FreeHookWrapper(IntPtr wrapper);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    private delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

    internal static readonly int HC_ACTION = 0,
        HC_GETNEXT = 1,
        HC_SKIP = 2,
        HC_NOREMOVE = 3,
        HC_SYSMODALON = 4,
        HC_SYSMODALOFF = 5;

    #endregion
}

/// <summary>
///     Hook Types. See the documentation of SetWindowsHookEx for reference.
/// </summary>
public enum HookType {
    ///
    WH_JOURNALRECORD = 0,
    ///
    WH_JOURNALPLAYBACK = 1,
    ///
    WH_KEYBOARD = 2,
    ///
    WH_GETMESSAGE = 3,
    ///
    WH_CALLWNDPROC = 4,
    ///
    WH_CBT = 5,
    ///
    WH_SYSMSGFILTER = 6,
    ///
    WH_MOUSE = 7,
    ///
    WH_HARDWARE = 8,
    ///
    WH_DEBUG = 9,
    ///
    WH_SHELL = 10,
    ///
    WH_FOREGROUNDIDLE = 11,
    ///
    WH_CALLWNDPROCRET = 12,
    ///
    WH_KEYBOARD_LL = 13,
    ///
    WH_MOUSE_LL = 14
}
