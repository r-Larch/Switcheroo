using System.Collections;


namespace Switcheroo.Windows;

public class WindowCollection : IReadOnlyCollection<AppWindow> {
    private readonly List<AppWindow> _list = new(20);

    public void Add(AppWindow appWindow)
    {
        _list.Add(appWindow);
    }

    public IEnumerator<AppWindow> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _list.Count;
}
