namespace Switcheroo.Windows;

public readonly struct HWnd : IEquatable<HWnd> {
    private readonly nint _hWnd;
    internal HWnd(nint hWnd) => _hWnd = hWnd;

    #region Equality

    public bool Equals(HWnd other) => _hWnd.Equals(other._hWnd);
    public override bool Equals(object? obj) => obj is HWnd other && Equals(other);
    public override int GetHashCode() => _hWnd.GetHashCode();
    public static bool operator ==(HWnd h1, HWnd h2) => h1._hWnd == h2._hWnd;
    public static bool operator !=(HWnd h1, HWnd h2) => h1._hWnd != h2._hWnd;
    public static implicit operator HWnd(nint hWnd) => new(hWnd);
    public static implicit operator nint(HWnd hWnd) => hWnd._hWnd;
    public override string ToString() => _hWnd.ToString();

    #endregion
}
