using System;

namespace StateApp;

public interface IState { void Handle(Context ctx); }

public class Context
{
    public IState State { get; set; }
    public Context(IState state) { State = state; }
    public void Request() => State.Handle(this);
}

public class ConcreteStateA : IState
{
    public void Handle(Context ctx)
    {
        Console.WriteLine("State A handling and switching to B");
        ctx.State = new ConcreteStateB();
    }
}

public class ConcreteStateB : IState
{
    public void Handle(Context ctx)
    {
        Console.WriteLine("State B handling and switching to A");
        ctx.State = new ConcreteStateA();
    }
}
