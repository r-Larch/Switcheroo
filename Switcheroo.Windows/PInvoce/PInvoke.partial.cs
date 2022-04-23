using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Switcheroo.Windows;


// ReSharper disable once CheckNamespace
namespace Windows.Win32;

internal static partial class PInvoke {
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong_x86(nint hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtrImpl_x64(nint hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    public static unsafe nint GetWindowLongPtr(nint hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        return sizeof(nint) == 4 ? GetWindowLong_x86(hWnd, nIndex) : GetWindowLongPtrImpl_x64(hWnd, nIndex);
    }
}

internal static partial class PInvoke {
    /// <summary>Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function. EnumWindows continues until the last top-level window is enumerated or the callback function returns FALSE.</summary>
    /// <param name="lpEnumFunc">
    /// <para>Type: <b>WNDENUMPROC</b> A pointer to an application-defined callback function. For more information, see <a href="https://docs.microsoft.com/previous-versions/windows/desktop/legacy/ms633498(v=vs.85)">EnumWindowsProc</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumwindows#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lParam">
    /// <para>Type: <b>LPARAM</b> An application-defined value to be passed to the callback function.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumwindows#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>. If <a href="/previous-versions/windows/desktop/legacy/ms633498(v=vs.85)">EnumWindowsProc</a> returns zero, the return value is also zero. In this case, the callback function should call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-setlasterror">SetLastError</a> to obtain a meaningful error code to be returned to the caller of <b>EnumWindows</b>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumwindows">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe BOOL EnumWindows(delegate* unmanaged<HWnd, nint, BOOL> lpEnumFunc, nint lParam);

    /// <summary>Enumerates the child windows that belong to the specified parent window by passing the handle to each child window, in turn, to an application-defined callback function.</summary>
    /// <param name="hWndParent">
    /// <para>Type: <b>HWND</b> A handle to the parent window whose child windows are to be enumerated. If this parameter is <b>NULL</b>, this function is equivalent to <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-enumwindows">EnumWindows</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumchildwindows#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpEnumFunc">
    /// <para>Type: <b>WNDENUMPROC</b> A pointer to an application-defined callback function. For more information, see <a href="https://docs.microsoft.com/previous-versions/windows/desktop/legacy/ms633493(v=vs.85)">EnumChildProc</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumchildwindows#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lParam">
    /// <para>Type: <b>LPARAM</b> An application-defined value to be passed to the callback function.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumchildwindows#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> The return value is not used.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-enumchildwindows">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe BOOL EnumChildWindows(HWnd hWndParent, delegate* unmanaged<HWnd, nint, BOOL> lpEnumFunc, nint lParam);

    /// <summary>Determines the visibility state of the specified window.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window to be tested.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-iswindowvisible#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the specified window, its parent window, its parent's parent window, and so forth, have the <b>WS_VISIBLE</b> style, the return value is nonzero. Otherwise, the return value is zero. Because the return value specifies whether the window has the <b>WS_VISIBLE</b> style, it may be nonzero even if the window is totally obscured by other windows.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-iswindowvisible">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern BOOL IsWindowVisible(HWnd hWnd);

    /// <summary>Retrieves a data handle from the property list of the specified window. The character string identifies the handle to be retrieved. The string and handle must have been added to the property list by a previous call to the SetProp function.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window whose property list is to be searched.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getpropw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpString">
    /// <para>Type: <b>LPCTSTR</b> An atom that identifies a string. If this parameter is an atom, it must have been created by using the <a href="https://docs.microsoft.com/windows/desktop/api/winbase/nf-winbase-globaladdatoma">GlobalAddAtom</a> function. The atom, a 16-bit value, must be placed in the low-order word of the <i>lpString</i> parameter; the high-order word must be zero.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getpropw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>HANDLE</b> If the property list contains the string, the return value is the associated data handle. Otherwise, the return value is <b>NULL</b>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getpropw">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetPropW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe nint GetProp(HWnd hWnd, char* lpString);

    /// <summary>Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to a window. The window handle retrieved is relative to this window, based on the value of the <i>uCmd</i> parameter.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindow#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="uCmd">Type: <b>UINT</b></param>
    /// <returns>
    /// <para>Type: <b>HWND</b> If the function succeeds, the return value is a window handle. If no window exists with the specified relationship to the specified window, the return value is <b>NULL</b>. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindow">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern HWnd GetWindow(HWnd hWnd, GET_WINDOW_CMD uCmd);

    /// <summary>Determines whether the specified window is enabled for mouse and keyboard input.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window to be tested.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-iswindowenabled#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the window is enabled, the return value is nonzero. If the window is not enabled, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-iswindowenabled">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern BOOL IsWindowEnabled(HWnd hWnd);

    /// <summary>Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar).</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window or control.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextlengthw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>int</b> If the function succeeds, the return value is the length, in characters, of the text. Under certain conditions, this value might be greater than the length of the text (see Remarks). If the window has no text, the return value is zero. Function failure is indicated by a return value of zero and a <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a> result that is nonzero. > [!NOTE] > This function does not clear the most recent error information. To determine success or failure, clear the most recent error information by calling <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-setlasterror">SetLastError</a> with 0, then call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextlengthw">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern int GetWindowTextLength(HWnd hWnd);

    /// <summary>Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another application.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window or control containing the text.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpString">
    /// <para>Type: <b>LPTSTR</b> The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="nMaxCount">
    /// <para>Type: <b>int</b> The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>int</b> If the function succeeds, the return value is the length, in characters, of the copied string, not including the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window or control handle is invalid, the return value is zero. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>. This function cannot retrieve the text of an edit control in another application.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowtextw">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetWindowTextW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe int GetWindowText(HWnd hWnd, char* lpString, int nMaxCount);

    /// <summary>Retrieves the name of the class to which the specified window belongs.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window and, indirectly, the class to which the window belongs.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclassnamew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpClassName">
    /// <para>Type: <b>LPTSTR</b> The class name string.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclassnamew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="nMaxCount">
    /// <para>Type: <b>int</b> The length of the *lpClassName* buffer, in characters. The buffer must be large enough to include the terminating null character; otherwise, the class name string is truncated to `nMaxCount-1` characters.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclassnamew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>int</b> If the function succeeds, the return value is the number of characters copied to the buffer, not including the terminating null character. If the function fails, the return value is zero. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getclassnamew">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "GetClassNameW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe int GetClassName(HWnd hWnd, char* lpClassName, int nMaxCount);

    /// <summary>Retrieves the current value of a specified Desktop Window Manager (DWM) attribute applied to a window.</summary>
    /// <param name="hwnd">The handle to the window from which the attribute value is to be retrieved.</param>
    /// <param name="dwAttribute">A flag describing which value to retrieve, specified as a value of the [DWMWINDOWATTRIBUTE](/windows/desktop/api/dwmapi/ne-dwmapi-dwmwindowattribute) enumeration. This parameter specifies which attribute to retrieve, and the *pvAttribute* parameter points to an object into which the attribute value is retrieved.</param>
    /// <param name="pvAttribute">A pointer to a value which, when this function returns successfully, receives the current value of the attribute. The type of the retrieved value depends on the value of the *dwAttribute* parameter. The [**DWMWINDOWATTRIBUTE**](/windows/desktop/api/Dwmapi/ne-dwmapi-dwmwindowattribute) enumeration topic indicates, in the row for each flag, what type of value you should pass a pointer to in the *pvAttribute* parameter.</param>
    /// <param name="cbAttribute">The size, in bytes, of the attribute value being received via the *pvAttribute* parameter. The type of the retrieved value, and therefore its size in bytes, depends on the value of the *dwAttribute* parameter.</param>
    /// <returns>
    /// <para>Type: **[HRESULT](/windows/desktop/com/structure-of-com-error-codes)** If the function succeeds, it returns **S_OK**. Otherwise, it returns an [**HRESULT**](/windows/desktop/com/structure-of-com-error-codes) [error code](/windows/desktop/com/com-error-codes-10).</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//dwmapi/nf-dwmapi-dwmgetwindowattribute">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("DwmApi", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows6.0.6000")]
    internal static extern unsafe HRESULT DwmGetWindowAttribute(HWnd hwnd, Graphics.Dwm.DWMWINDOWATTRIBUTE dwAttribute, void* pvAttribute, uint cbAttribute);

    /// <summary>Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.</summary>
    /// <returns>
    /// <para>Type: <b>HWND</b> The return value is a handle to the foreground window. The foreground window can be <b>NULL</b> in certain circumstances, such as when a window is losing activation.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getforegroundwindow">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern HWnd GetForegroundWindow();

    /// <summary>Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowthreadprocessid#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lpdwProcessId">
    /// <para>Type: <b>LPDWORD</b> A pointer to a variable that receives the process identifier. If this parameter is not <b>NULL</b>, <b>GetWindowThreadProcessId</b> copies the identifier of the process to the variable; otherwise, it does not.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowthreadprocessid#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>DWORD</b> The return value is the identifier of the thread that created the window.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getwindowthreadprocessid">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern unsafe uint GetWindowThreadProcessId(HWnd hWnd, [Optional] uint* lpdwProcessId);

    /// <summary>Places (posts) a message in the message queue associated with the thread that created the specified window and returns without waiting for the thread to process the message.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window whose window procedure is to receive the message. The following values have special meanings. </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-postmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="Msg">
    /// <para>Type: <b>UINT</b> The message to be posted. For lists of the system-provided messages, see <a href="https://docs.microsoft.com/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-postmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="wParam">
    /// <para>Type: <b>WPARAM</b> Additional message-specific information.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-postmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lParam">
    /// <para>Type: <b>LPARAM</b> Additional message-specific information.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-postmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>. <b>GetLastError</b> returns <b>ERROR_NOT_ENOUGH_QUOTA</b> when the limit is hit.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-postmessagew">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "PostMessageW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern BOOL PostMessage(HWnd hWnd, uint Msg, nint wParam, nint lParam);

    /// <summary>Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window whose window procedure will receive the message. If this parameter is <b>HWND_BROADCAST</b> ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows. Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="Msg">
    /// <para>Type: <b>UINT</b> The message to be sent. For lists of the system-provided messages, see <a href="https://docs.microsoft.com/windows/desktop/winmsg/about-messages-and-message-queues">System-Defined Messages</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="wParam">
    /// <para>Type: <b>WPARAM</b> Additional message-specific information.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lParam">
    /// <para>Type: <b>LPARAM</b> Additional message-specific information.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>LRESULT</b> The return value specifies the result of the message processing; it depends on the message sent.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-sendmessagew">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, EntryPoint = "SendMessageW", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern LRESULT SendMessage(HWnd hWnd, uint Msg, nint wParam, nint lParam);

    /// <summary>Switches focus to the specified window and brings it to the foreground.</summary>
    /// <param name="hwnd">
    /// <para>Type: <b>HWND</b> A handle to the window.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-switchtothiswindow#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="fUnknown">
    /// <para>Type: <b>BOOL</b> A <b>TRUE</b> for this parameter indicates that the window is being switched to using the Alt/Ctl+Tab key sequence.  This parameter should be <b>FALSE</b> otherwise.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-switchtothiswindow#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <remarks>
    /// <para>This function is typically called to maintain window z-ordering. This function was not included in the SDK headers and libraries until Windows XP with Service Pack 1 (SP1) and Windows Server 2003. If you do not have a header file and import library for this function, you can call the function using <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibrarya">LoadLibrary</a> and <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-getprocaddress">GetProcAddress</a>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-switchtothiswindow#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern void SwitchToThisWindow(HWnd hwnd, BOOL fUnknown);

    /// <summary>Retrieves the handle to the ancestor of the specified window.</summary>
    /// <param name="hwnd">
    /// <para>Type: <b>HWND</b> A handle to the window whose ancestor is to be retrieved. If this parameter is the desktop window, the function returns <b>NULL</b>.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getancestor#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="gaFlags">Type: <b>UINT</b></param>
    /// <returns>
    /// <para>Type: <b>HWND</b> The return value is the handle to the ancestor window.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getancestor">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern HWnd GetAncestor(HWnd hwnd, GET_ANCESTOR_FLAGS gaFlags);

    /// <summary>Determines which pop-up window owned by the specified window was most recently active.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the owner window.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getlastactivepopup#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>HWND</b> The return value identifies the most recently active pop-up window. The return value is the same as the <i>hWnd</i> parameter, if any of the following conditions are met: </para>
    /// <para>This doc was truncated.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getlastactivepopup">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern HWnd GetLastActivePopup(HWnd hWnd);

    /// <summary>Retrieves a handle to the specified window's parent or owner.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window whose parent window handle is to be retrieved.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getparent#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>HWND</b> If the window is a child window, the return value is a handle to the parent window. If the window is a top-level window with the <b>WS_POPUP</b> style, the return value is a handle to the owner window. If the function fails, the return value is <b>NULL</b>. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>. This function typically fails for one of the following reasons:</para>
    /// <para></para>
    /// <para>This doc was truncated.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-getparent">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern HWnd GetParent(HWnd hWnd);

    /// <summary>Brings the thread that created the specified window into the foreground and activates the window.</summary>
    /// <param name="hWnd">
    /// <para>Type: <b>HWND</b> A handle to the window that should be activated and brought to the foreground.</para>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setforegroundwindow#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the window was brought to the foreground, the return value is nonzero.</para>
    /// <para>If the window was not brought to the foreground, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    /// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/nf-winuser-setforegroundwindow">Learn more about this API from docs.microsoft.com</see>.</para>
    /// </remarks>
    [DllImport("User32", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    internal static extern BOOL SetForegroundWindow(HWnd hWnd);
}
