namespace FacadeApp;

public class SubsystemA { public string A() => "SubsystemA: Ready"; }
public class SubsystemB { public string B() => "SubsystemB: Go"; }
public class SubsystemC { public string C() => "SubsystemC: Set"; }

public class Facade
{
    private readonly SubsystemA _a = new();
    private readonly SubsystemB _b = new();
    private readonly SubsystemC _c = new();

    public string Operation()
    {
        return _a.A() + ", " + _b.B() + ", " + _c.C();
    }
}
