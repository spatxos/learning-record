using System;

namespace MediatorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Mediator Pattern Demo:\n");
        var mediator = new ConcreteMediator();
        var c1 = new Component1(mediator);
        var c2 = new Component2(mediator);
        mediator.C1 = c1; mediator.C2 = c2;
        c1.DoB();
        c2.DoD();
    }
}
