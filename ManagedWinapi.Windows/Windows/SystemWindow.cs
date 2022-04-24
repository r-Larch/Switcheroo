using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace ManagedWinapi.Windows;

/// <summary>
///     Represents any window used by Windows, including those from other applications.
/// </summary>
public class SystemWindow {
    private static readonly Func<SystemWindow, bool> All = _ => true;

    private static readonly Regex ProcessNameRegex = new("([^\\\\]+?)(?=(\\.exe)?$)");

    private readonly IntPtr _hwnd;

    private bool _isClosed;

    /// <summary>
    ///     Create a new SystemWindow instance from a window handle.
    /// </summary>
    /// <param name="hWnd">The window handle.</param>
    public SystemWindow(IntPtr hWnd)
    {
        _hwnd = hWnd;
    }

    /// <summary>
    ///     Allows getting the current foreground window and setting it.
    /// </summary>
    public static SystemWindow ForegroundWindow {
        get => new(GetForegroundWindow());
        set => SetForegroundWindow(value.HWnd);
    }

    /// <summary>
    ///     The Desktop window, i. e. the window that covers the
    ///     complete desktop.
    /// </summary>
    public static SystemWindow DesktopWindow => new(GetDesktopWindow());

    /// <summary>
    ///     Returns all available top level windows.
    /// </summary>
    public static IReadOnlyCollection<SystemWindow> AllTopLevelWindows => FilterTopLevelWindows(All);

    /// <summary>
    ///     Return all descendant windows (child windows and their descendants).
    /// </summary>
    public IReadOnlyCollection<SystemWindow> AllDescendantWindows => FilterDescendantWindows(false, All);

    /// <summary>
    ///     Return all direct child windows.
    /// </summary>
    public IReadOnlyCollection<SystemWindow> AllChildWindows => FilterDescendantWindows(true, All);

    /// <summary>
    ///     The Window handle of this window.
    /// </summary>
    public IntPtr HWnd => _hwnd;

    /// <summary>
    ///     The title of this window (by the <c>GetWindowText</c> API function).
    /// </summary>
    public string Title {
        get {
            var sb = new StringBuilder(GetWindowTextLength(_hwnd) + 1);
            GetWindowText(_hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        set => SetWindowText(_hwnd, value);
    }

    /// <summary>
    ///     The text inside of this window (by sending a <c>WM_GETTEXT</c> message).
    ///     For child windows of other applications, this is more reliable
    ///     than the <see cref="Title" /> function.
    /// </summary>
    public string Text {
        get {
            var length = SendGetMessage(WM_GETTEXTLENGTH);
            var sb = new StringBuilder(length + 1);
            SendMessage(new HandleRef(this, HWnd), WM_GETTEXT, new IntPtr(sb.Capacity), sb);
            return sb.ToString();
        }
    }

    /// <summary>
    ///     The name of the window class (by the <c>GetClassName</c> API function).
    ///     This class has nothing to do with classes in C# or other .NET languages.
    /// </summary>
    public string ClassName {
        get {
            var length = 64;
            while (true) {
                var sb = new StringBuilder(length);
                ApiHelper.FailIfZero(GetClassName(_hwnd, sb, sb.Capacity));
                if (sb.Length != length - 1) {
                    return sb.ToString();
                }

                length *= 2;
            }
        }
    }

    /// <summary>
    ///     Whether this window is currently visible. A window is visible if its
    ///     and all ancestor's visibility flags are true.
    /// </summary>
    public bool Visible => IsWindowVisible(_hwnd);

    /// <summary>
    ///     Whether this window always appears above all other windows
    ///     that do not have this property set to true.
    /// </summary>
    public bool TopMost {
        get => (ExtendedStyle & WindowExStyleFlags.TOPMOST) != 0;
        set {
            if (value) {
                SetWindowPos(_hwnd, new IntPtr(-1), 0, 0, 0, 0, 3);
            }
            else {
                SetWindowPos(_hwnd, new IntPtr(-2), 0, 0, 0, 0, 3);
            }
        }
    }

    /// <summary>
    ///     Whether this window is currently enabled (able to accept keyboard input).
    /// </summary>
    public bool Enabled {
        get => IsWindowEnabled(_hwnd);
        set => EnableWindow(_hwnd, value);
    }
    public bool IsClosed {
        get {
            _isClosed = _isClosed || GetClassNameFails();
            return _isClosed;
        }
    }

    public bool IsClosedOrHidden => IsClosed || !Visible;

    /// <summary>
    ///     Returns or sets the visibility flag.
    /// </summary>
    /// <seealso cref="SystemWindow.Visible" />
    public bool VisibilityFlag {
        get => (Style & WindowStyleFlags.VISIBLE) != 0;
        set {
            if (value) {
                ShowWindow(_hwnd, 5);
            }
            else {
                ShowWindow(_hwnd, 0);
            }
        }
    }

    /// <summary>
    ///     This window's style flags.
    /// </summary>
    public WindowStyleFlags Style {
        get => (WindowStyleFlags) (long) GetWindowLongPtr(_hwnd, (int) (GWL.GWL_STYLE));
        set => SetWindowLong(_hwnd, (int) GWL.GWL_STYLE, (int) value);
    }

    /// <summary>
    ///     This window's extended style flags.
    /// </summary>
    public WindowExStyleFlags ExtendedStyle {
        get => (WindowExStyleFlags) GetWindowLongPtr(_hwnd, (int) (GWL.GWL_EXSTYLE));
        set => SetWindowLong(_hwnd, (int) GWL.GWL_EXSTYLE, (int) value);
    }

    /// <summary>
    ///     This window's parent. A dialog's parent is its owner, a component's parent is
    ///     the window that contains it.
    /// </summary>
    public SystemWindow Parent => new(GetParent(_hwnd));

    /// <summary>
    ///     The window's parent, but only if this window is its parent child. Some
    ///     parents, like dialog owners, do not have the window as its child. In that case,
    ///     <c>null</c> will be returned.
    /// </summary>
    public SystemWindow? ParentSymmetric {
        get {
            var result = Parent;
            if (!IsDescendantOf(result)) result = null;
            return result;
        }
    }

    /// <summary>
    ///     The window's owner
    /// </summary>
    public SystemWindow Owner {
        get {
            var owner = GetWindow(HWnd, (uint) GetWindow_Cmd.GW_OWNER);
            return new SystemWindow(owner);
        }
    }

    /// <summary>
    ///     The window's position inside its parent or on the screen.
    /// </summary>
    public RECT Position {
        get {
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(_hwnd, ref wp);
            return wp.rcNormalPosition;
        }

        set {
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(_hwnd, ref wp);
            wp.rcNormalPosition = value;
            SetWindowPlacement(_hwnd, ref wp);
        }
    }

    /// <summary>
    ///     The window's location inside its parent or on the screen.
    /// </summary>
    public Point Location {
        get => Position.Location;

        set {
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(_hwnd, ref wp);
            wp.rcNormalPosition.Bottom = value.Y + wp.rcNormalPosition.Height;
            wp.rcNormalPosition.Right = value.X + wp.rcNormalPosition.Width;
            wp.rcNormalPosition.Top = value.Y;
            wp.rcNormalPosition.Left = value.X;
            SetWindowPlacement(_hwnd, ref wp);
        }
    }

    /// <summary>
    ///     The window's size.
    /// </summary>
    public Size Size {
        get => Position.Size;

        set {
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(_hwnd, ref wp);
            wp.rcNormalPosition.Right = wp.rcNormalPosition.Left + value.Width;
            wp.rcNormalPosition.Bottom = wp.rcNormalPosition.Top + value.Height;
            SetWindowPlacement(_hwnd, ref wp);
        }
    }

    /// <summary>
    ///     The window's position in absolute screen coordinates. Use
    ///     <see cref="Position" /> if you want to use the relative position.
    /// </summary>
    public RECT Rectangle {
        get {
            GetWindowRect(_hwnd, out var r);
            return r;
        }
    }

    /// <summary>
    ///     The ID of the process which created this window.
    /// </summary>
    public int ProcessId {
        get {
            GetWindowThreadProcessId(HWnd, out var pid);
            return pid;
        }
    }

    /// <summary>
    ///     The name of the process which created this window.
    /// </summary>
    public string ProcessName {
        get {
            var handle = OpenProcess(ProcessRights.PROCESS_QUERY_INFORMATION | ProcessRights.PROCESS_VM_READ, false, ProcessId);
            var exeBufferSize = 512;
            var exeBuffer = new StringBuilder(exeBufferSize);
            QueryFullProcessImageName(handle, 0, exeBuffer, out exeBufferSize);
            return ProcessNameRegex.Match(exeBuffer.ToString()).Value;
        }
    }

    /// <summary>
    ///     The Thread which created this window.
    /// </summary>
    public ProcessThread Thread {
        get {
            var tid = GetWindowThreadProcessId(HWnd, out var pid);
            foreach (ProcessThread t in Process.GetProcessById(pid).Threads) {
                if (t.Id == tid) return t;
            }

            throw new Exception("Thread not found");
        }
    }

    /// <summary>
    ///     Whether this window is minimized or maximized.
    /// </summary>
    public FormWindowState WindowState {
        get {
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(HWnd, ref wp);
            switch (wp.showCmd % 4) {
                case 2: return FormWindowState.Minimized;
                case 3: return FormWindowState.Maximized;
                default: return FormWindowState.Normal;
            }
        }
        set {
            int showCommand;
            switch (value) {
                case FormWindowState.Normal:
                    showCommand = 1;
                    break;
                case FormWindowState.Minimized:
                    showCommand = 2;
                    break;
                case FormWindowState.Maximized:
                    showCommand = 3;
                    break;
                default: return;
            }

            ShowWindow(HWnd, showCommand);
        }
    }

    /// <summary>
    ///     Whether this window can be moved on the screen by the user.
    /// </summary>
    public bool Movable => (Style & WindowStyleFlags.SYSMENU) != 0;

    /// <summary>
    ///     Whether this window can be resized by the user. Resizing a window that
    ///     cannot be resized by the user works, but may be irritating to the user.
    /// </summary>
    public bool Resizable => (Style & WindowStyleFlags.THICKFRAME) != 0;

    /// <summary>
    ///     An image of this window. Unlike a screen shot, this will not
    ///     contain parts of other windows (partially) cover this window.
    ///     If you want to create a screen shot, use the
    ///     <see cref="System.Drawing.Graphics.CopyFromScreen(System.Drawing.Point,System.Drawing.Point,System.Drawing.Size)" />
    ///     function and use the <see cref="SystemWindow.Rectangle" /> property for
    ///     the range.
    /// </summary>
    public Image Image {
        get {
            var bmp = new Bitmap(Position.Width, Position.Height);
            var g = Graphics.FromImage(bmp);
            var pTarget = g.GetHdc();
            var pSource = CreateCompatibleDC(pTarget);
            var pOrig = SelectObject(pSource, bmp.GetHbitmap());
            PrintWindow(HWnd, pTarget, 0);
            var pNew = SelectObject(pSource, pOrig);
            DeleteObject(pOrig);
            DeleteObject(pNew);
            DeleteObject(pSource);
            g.ReleaseHdc(pTarget);
            g.Dispose();
            return bmp;
        }
    }

    /// <summary>
    ///     The window's visible region.
    /// </summary>
    public Region? Region {
        get {
            var rgn = CreateRectRgn(0, 0, 0, 0);
            var r = GetWindowRgn(HWnd, rgn);
            if (r == (int) GetWindowRegnReturnValues.ERROR) {
                return null;
            }

            return Region.FromHrgn(rgn);
        }
        set {
            var bmp = new Bitmap(1, 1);
            var g = Graphics.FromImage(bmp);
            var hRgn = value?.GetHrgn(g) ?? IntPtr.Zero;
            SetWindowRgn(HWnd, hRgn, true);
            g.Dispose();
        }
    }

    /// <summary>
    ///     The character used to mask passwords, if this control is
    ///     a text field. May be used for different purpose by other
    ///     controls.
    /// </summary>
    public char PasswordCharacter {
        get => (char) SendGetMessage(EM_GETPASSWORDCHAR);
        set => SendSetMessage(EM_SETPASSWORDCHAR, value);
    }

    /// <summary>
    ///     The ID of a control within a dialog. This is used in
    ///     WM_COMMAND messages to distinguish which control sent the command.
    /// </summary>
    public int DialogId => GetWindowLong32(_hwnd, (int) GWL.GWL_ID);

    /// <summary>
    ///     Get the window that is below this window in the Z order,
    ///     or null if this is the lowest window.
    /// </summary>
    public SystemWindow? WindowBelow {
        get {
            var res = GetWindow(HWnd, (uint) GetWindow_Cmd.GW_HWNDNEXT);
            if (res == IntPtr.Zero) return null;
            return new SystemWindow(res);
        }
    }

    /// <summary>
    ///     Get the window that is above this window in the Z order,
    ///     or null, if this is the foreground window.
    /// </summary>
    public SystemWindow? WindowAbove {
        get {
            var res = GetWindow(HWnd, (uint) GetWindow_Cmd.GW_HWNDPREV);
            if (res == IntPtr.Zero) return null;
            return new SystemWindow(res);
        }
    }

    /// <summary>
    ///     Whether this control, which is a check box or radio button, is checked.
    /// </summary>
    public CheckState CheckState {
        get => (CheckState) SendGetMessage(BM_GETCHECK);
        set => SendSetMessage(BM_SETCHECK, (uint) value);
    }

    /// <summary>
    ///     Returns all top level windows that match the given predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter.</param>
    /// <returns>The filtered top level windows</returns>
    public static IReadOnlyCollection<SystemWindow> FilterTopLevelWindows(Func<SystemWindow, bool> predicate)
    {
        var windows = new List<SystemWindow>();
        EnumWindows(delegate(IntPtr hwnd, IntPtr lParam) {
            var tmp = new SystemWindow(hwnd);
            if (predicate(tmp))
                windows.Add(tmp);
            return 1;
        }, new IntPtr(0));
        return windows;
    }

    /// <summary>
    ///     Finds the system window below the given point. This need not be a
    ///     top level window; disabled windows are not returned either.
    ///     If you have problems with transparent windows that cover nontransparent
    ///     windows, consider using <see cref="FromPointEx" />, since that method
    ///     tries hard to avoid this problem.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns></returns>
    public static SystemWindow? FromPoint(int x, int y)
    {
        var hwnd = WindowFromPoint(new POINT(x, y));
        if (hwnd.ToInt64() == 0) {
            return null;
        }

        return new SystemWindow(hwnd);
    }

    /// <summary>
    ///     Finds the system window below the given point. This method uses a more
    ///     sophisticated algorithm than <see cref="FromPoint" />, but is slower.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="topLevel">Whether to return the top level window.</param>
    /// <param name="enabledOnly">Whether to return enabled windows only.</param>
    /// <returns></returns>
    public static SystemWindow? FromPointEx(int x, int y, bool topLevel, bool enabledOnly)
    {
        var sw = FromPoint(x, y);
        if (sw == null) return null;

        while (sw.ParentSymmetric != null) {
            sw = sw.ParentSymmetric;
        }

        if (topLevel) {
            return sw;
        }

        var area = GetArea(sw);
        var result = sw;
        foreach (var w in sw.AllDescendantWindows) {
            if (w.Visible && (w.Enabled || !enabledOnly)) {
                if (w.Rectangle.ToRectangle().Contains(x, y)) {
                    var ar2 = GetArea(w);
                    if (ar2 <= area) {
                        area = ar2;
                        result = w;
                    }
                }
            }
        }

        return result;

        static int GetArea(SystemWindow sw)
        {
            var rr = sw.Rectangle;
            return rr.Height * rr.Width;
        }
    }


    /// <summary>
    ///     Returns all child windows that match the given predicate.
    /// </summary>
    /// <param name="directOnly">Whether to include only direct children (no descendants)</param>
    /// <param name="predicate">The predicate to filter.</param>
    /// <returns>The list of child windows.</returns>
    public IReadOnlyCollection<SystemWindow> FilterDescendantWindows(bool directOnly, Func<SystemWindow, bool> predicate)
    {
        var windows = new List<SystemWindow>();
        EnumChildWindows(_hwnd, delegate(IntPtr hwnd, IntPtr lParam) {
            var tmp = new SystemWindow(hwnd);
            var add = true;
            if (directOnly) {
                add = tmp.Parent._hwnd == _hwnd;
            }

            if (add && predicate(tmp))
                windows.Add(tmp);
            return 1;
        }, new IntPtr(0));
        return windows;
    }

    private bool GetClassNameFails()
    {
        var builder = new StringBuilder(2);
        return GetClassName(HWnd, builder, builder.Capacity) == 0;
    }

    /// <summary>
    ///     Check whether this window is a descendant of <c>ancestor</c>
    /// </summary>
    /// <param name="ancestor">The suspected ancestor</param>
    /// <returns>If this is really an ancestor</returns>
    public bool IsDescendantOf(SystemWindow ancestor)
    {
        return IsChild(ancestor._hwnd, _hwnd);
    }

    /// <summary>
    ///     Gets a device context for this window.
    /// </summary>
    /// <param name="clientAreaOnly">
    ///     Whether to get the context for
    ///     the client area or for the full window.
    /// </param>
    public WindowDeviceContext GetDeviceContext(bool clientAreaOnly)
    {
        if (clientAreaOnly) {
            return new WindowDeviceContext(this, GetDC(_hwnd));
        }

        return new WindowDeviceContext(this, GetWindowDC(_hwnd));
    }

    /// <summary>
    ///     Whether this SystemWindow represents a valid window that existed
    ///     when this SystemWindow instance was created. To check if a window
    ///     still exists, better check its <see cref="ClassName" /> property.
    /// </summary>
    public bool IsValid()
    {
        return _hwnd != IntPtr.Zero;
    }

    /// <summary>
    ///     Send a message to this window that it should close. This is equivalent
    ///     to clicking the "X" in the upper right corner or pressing Alt+F4.
    /// </summary>
    public void SendClose()
    {
        SendSetMessage(WM_CLOSE, 0);
    }

    /// <summary>
    ///     Post a message to this window that it should close. This is equivalent
    ///     to clicking the "X" in the upper right corner or pressing Alt+F4.
    ///     It sometimes works in instances where the <see cref="SendClose" /> function does
    ///     not (for example, Windows Explorer windows.)
    /// </summary>
    public void PostClose()
    {
        PostMessage(HWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    /// <summary>
    ///     Closes the window by sending the "WM_SYSCOMMAND" with the "SC_CLOSE" parameter.
    ///     This equals that the user open the Window menu and click "Close". This method
    ///     seem to work in more scenaries than "SendClose()" and "PostClose()".
    ///     Also see: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
    /// </summary>
    public void Close()
    {
        PostMessage(HWnd, WM_SYSCOMMAND, SC_CLOSE, IntPtr.Zero);
    }

    /// <summary>
    ///     Highlights the window with a red border.
    /// </summary>
    public void Highlight()
    {
        GetWindowRect(_hwnd, out var rect);
        using var windowDc = GetDeviceContext(false);
        using (var g = windowDc.CreateGraphics()) {
            g.DrawRectangle(new Pen(Color.Red, 4), 0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }

    /// <summary>
    ///     Forces the window to invalidate its client area and immediately redraw itself and any child controls.
    /// </summary>
    public void Refresh()
    {
        // By using parent, we get better results in refreshing old drawing window area.
        var hwndToRefresh = _hwnd;
        var parentWindow = ParentSymmetric;
        if (parentWindow != null) {
            hwndToRefresh = parentWindow._hwnd;
        }

        InvalidateRect(hwndToRefresh, IntPtr.Zero, true);
        RedrawWindow(hwndToRefresh, IntPtr.Zero, IntPtr.Zero, RDW.RDW_FRAME | RDW.RDW_INVALIDATE | RDW.RDW_UPDATENOW | RDW.RDW_ALLCHILDREN | RDW.RDW_ERASENOW);
    }

    internal int SendGetMessage(uint message)
    {
        return SendGetMessage(message, 0);
    }

    internal int SendGetMessage(uint message, uint param)
    {
        return SendMessage(new HandleRef(this, HWnd), message, new IntPtr(param), new IntPtr(0)).ToInt32();
    }

    internal void SendSetMessage(uint message, uint value)
    {
        SendMessage(new HandleRef(this, HWnd), message, new IntPtr(value), new IntPtr(0));
    }


    #region Equals and HashCode

    ///
    public override bool Equals(object? obj)
    {
        if (obj == null) {
            return false;
        }

        var sw = obj as SystemWindow;
        return Equals(sw);
    }

    ///
    public bool Equals(SystemWindow? sw)
    {
        if ((object?) sw == null) {
            return false;
        }

        return _hwnd == sw._hwnd;
    }

    ///
    public override int GetHashCode()
    {
        // avoid exceptions
        return unchecked((int) _hwnd.ToInt64());
    }

    /// <summary>
    ///     Compare two instances of this class for equality.
    /// </summary>
    public static bool operator ==(SystemWindow? a, SystemWindow? b)
    {
        if (ReferenceEquals(a, b)) {
            return true;
        }

        if ((object?) a == null || (object?) b == null) {
            return false;
        }

        return a._hwnd == b._hwnd;
    }

    /// <summary>
    ///     Compare two instances of this class for inequality.
    /// </summary>
    public static bool operator !=(SystemWindow? a, SystemWindow? b)
    {
        return !(a == b);
    }

    #endregion


    #region PInvoke Declarations

    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo
    // ReSharper disable MemberCanBePrivate.Local

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    private enum GWL {
        GWL_WNDPROC = (-4),
        GWL_HINSTANCE = (-6),
        GWL_HWNDPARENT = (-8),
        GWL_STYLE = (-16),
        GWL_EXSTYLE = (-20),
        GWL_USERDATA = (-21),
        GWL_ID = (-12)
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT {
        public int length;
        public readonly int flags;
        public readonly int showCmd;
        public readonly POINT ptMinPosition;
        public readonly POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport("user32.dll")]
    private static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, int dwRop);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    private enum TernaryRasterOperations : uint {
        SRCCOPY = 0x00CC0020,
        SRCPAINT = 0x00EE0086,
        SRCAND = 0x008800C6,
        SRCINVERT = 0x00660046,
        SRCERASE = 0x00440328,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        PATCOPY = 0x00F00021,
        PATPAINT = 0x00FB0A09,
        PATINVERT = 0x005A0049,
        DSTINVERT = 0x00550009,
        BLACKNESS = 0x00000042,
        WHITENESS = 0x00FF0062
    }

    private enum GetWindowRegnReturnValues {
        ERROR = 0,
        NULLREGION = 1,
        SIMPLEREGION = 2,
        COMPLEXREGION = 3
    }

    private const uint EM_GETPASSWORDCHAR = 0xD2;
    private const uint EM_SETPASSWORDCHAR = 0xCC;
    private const uint BM_GETCHECK = 0xF0;
    private const uint BM_SETCHECK = 0xF1;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, [Out] StringBuilder lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll", SetLastError = false)]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    private const int WM_CLOSE = 16, WM_GETTEXT = 13, WM_GETTEXTLENGTH = 14, WM_SYSCOMMAND = 274;

    private readonly IntPtr SC_CLOSE = new(61536);

    private enum GetWindow_Cmd {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    [DllImport("user32.dll")]
    private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    [Flags]
    private enum RDW : uint {
        RDW_INVALIDATE = 0x0001,
        RDW_INTERNALPAINT = 0x0002,
        RDW_ERASE = 0x0004,

        RDW_VALIDATE = 0x0008,
        RDW_NOINTERNALPAINT = 0x0010,
        RDW_NOERASE = 0x0020,

        RDW_NOCHILDREN = 0x0040,
        RDW_ALLCHILDREN = 0x0080,

        RDW_UPDATENOW = 0x0100,
        RDW_ERASENOW = 0x0200,

        RDW_FRAME = 0x0400,
        RDW_NOFRAME = 0x0800,
    }

    [DllImport("user32.dll")]
    private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RDW flags);

    [Flags]
    private enum ProcessRights {
        PROCESS_TERMINATE = 0x1,
        PROCESS_CREATE_THREAD = 0x2,
        PROCESS_SET_SESSIONID = 0x4,
        PROCESS_VM_OPERATION = 0x8,
        PROCESS_VM_READ = 0x10,
        PROCESS_VM_WRITE = 0x20,
        PROCESS_DUP_HANDLE = 0x40,
        PROCESS_CREATE_PROCESS = 0x80,
        PROCESS_SET_QUOTA = 0x100,
        PROCESS_SET_INFORMATION = 0x200,
        PROCESS_QUERY_INFORMATION = 0x400,
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(ProcessRights dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

    #endregion
}
