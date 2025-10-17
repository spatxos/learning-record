using System.Collections.Generic;

namespace CommandApp;

public interface ICommand { void Execute(); void Undo(); }

public class Receiver
{
    public int State { get; private set; }
    public void Add(int x) => State += x;
    public void Sub(int x) => State -= x;
}

public class AddCommand : ICommand
{
    private readonly Receiver _r; private readonly int _v;
    public AddCommand(Receiver r, int v) { _r = r; _v = v; }
    public void Execute() => _r.Add(_v);
    public void Undo() => _r.Sub(_v);
}

public class Invoker
{
    private readonly Stack<ICommand> _history = new();
    public void ExecuteCommand(ICommand cmd) { cmd.Execute(); _history.Push(cmd); }
    public void Undo() { if (_history.Count > 0) _history.Pop().Undo(); }
}
