using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;


// ReSharper disable once CheckNamespace
namespace Windows.Win32;

internal static partial class PInvoke {
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong_x86(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtrImpl_x64(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    public static IntPtr GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        return IntPtr.Size == 4 ? (IntPtr) GetWindowLong_x86(hWnd, nIndex) : GetWindowLongPtrImpl_x64(hWnd, nIndex);
    }

    [DllImport("User32", ExactSpelling = true, EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong_x86(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong);

    [DllImport("User32", ExactSpelling = true, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr_x64(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong);

    public static IntPtr SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 4 ? (IntPtr) SetWindowLong_x86(hWnd, nIndex, (int) dwNewLong) : SetWindowLongPtr_x64(hWnd, nIndex, dwNewLong);
    }
}
