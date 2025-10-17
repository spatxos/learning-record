using System;

namespace StrategyApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Strategy Pattern Demo:\n");
        var ctx = new Context(new AddStrategy());
        Console.WriteLine("Add: " + ctx.DoOperation(3, 4));
        ctx.SetStrategy(new MultiplyStrategy());
        Console.WriteLine("Multiply: " + ctx.DoOperation(3, 4));
    }
}
