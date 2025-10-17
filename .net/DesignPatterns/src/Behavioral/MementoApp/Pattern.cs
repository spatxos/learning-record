using System.Collections.Generic;

namespace MementoApp;

public class Memento { public string State { get; } public Memento(string s) { State = s; } }

public class Originator
{
    public string? State { get; set; }
    public Memento Save() => new Memento(State ?? string.Empty);
    public void Restore(Memento m) => State = m.State;
}

public class Caretaker
{
    private readonly List<Memento> _history = new();
    public void Add(Memento m) => _history.Add(m);
    public Memento Get(int idx) => _history[idx];
}
