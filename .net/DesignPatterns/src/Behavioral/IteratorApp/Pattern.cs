using System.Collections;
using System.Collections.Generic;

namespace IteratorApp;

public class MyCollection : IEnumerable<int>
{
    private readonly List<int> _items = new();
    public void Add(int i) => _items.Add(i);
    public IEnumerator<int> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
