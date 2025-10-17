namespace StrategyApp;

public interface IStrategy { int Execute(int a, int b); }

public class AddStrategy : IStrategy { public int Execute(int a, int b) => a + b; }
public class MultiplyStrategy : IStrategy { public int Execute(int a, int b) => a * b; }

public class Context
{
    private IStrategy _strategy;
    public Context(IStrategy strategy) { _strategy = strategy; }
    public void SetStrategy(IStrategy s) => _strategy = s;
    public int DoOperation(int a, int b) => _strategy.Execute(a, b);
}
