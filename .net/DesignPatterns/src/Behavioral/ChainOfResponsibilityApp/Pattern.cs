namespace ChainOfResponsibilityApp;

public abstract class Handler
{
    protected Handler? Next;
    public void SetNext(Handler next) => Next = next;
    public abstract string Handle(int amount);
}

public class Manager : Handler
{
    public override string Handle(int amount)
    {
        if (amount <= 1000) return $"Manager approved {amount}";
        return Next?.Handle(amount) ?? $"Manager cannot handle {amount}";
    }
}

public class Director : Handler
{
    public override string Handle(int amount)
    {
        if (amount <= 10000) return $"Director approved {amount}";
        return Next?.Handle(amount) ?? $"Director cannot handle {amount}";
    }
}

public class CEO : Handler
{
    public override string Handle(int amount) => $"CEO approved {amount}";
}
