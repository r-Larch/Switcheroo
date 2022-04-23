namespace Switcheroo.Core {
    public interface IWindowText {
        bool IsForegroundWindow { get; }
        string WindowTitle { get; }
        string ProcessTitle { get; }
    }
}
