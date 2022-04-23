using System.Collections;


namespace Switcheroo.Windows;

public class WindowCollection : IReadOnlyCollection<Window> {
    private readonly List<Window> _list = new(20);

    public void Add(Window window)
    {
        _list.Add(window);
    }

    public IEnumerator<Window> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _list.Count;
}
