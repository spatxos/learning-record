using System;

namespace StateApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("State Pattern Demo:\n");
        var ctx = new Context(new ConcreteStateA());
        ctx.Request();
        ctx.Request();
        ctx.Request();
    }
}
