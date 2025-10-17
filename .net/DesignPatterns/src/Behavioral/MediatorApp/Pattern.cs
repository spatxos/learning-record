using System;

namespace MediatorApp;

public interface IMediator { void Notify(object sender, string evt); }

public class ConcreteMediator : IMediator
{
    public Component1? C1 { get; set; }
    public Component2? C2 { get; set; }
    public void Notify(object sender, string evt)
    {
        if (evt == "A") C2?.DoC();
        if (evt == "B") C1?.DoA();
    }
}

public class Component1
{
    private readonly IMediator _m;
    public Component1(IMediator m) { _m = m; }
    public void DoA() => Console.WriteLine("Component1 does A");
    public void DoB() => _m.Notify(this, "A");
}

public class Component2
{
    private readonly IMediator _m;
    public Component2(IMediator m) { _m = m; }
    public void DoC() => Console.WriteLine("Component2 does C");
    public void DoD() => _m.Notify(this, "B");
}
