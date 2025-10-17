using System;

namespace VisitorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Visitor Pattern Demo:\n");
        var elements = new IElement[] { new ConcreteElementA(), new ConcreteElementB() };
        var visitor = new ConcreteVisitor();
        foreach (var e in elements) e.Accept(visitor);
    }
}
