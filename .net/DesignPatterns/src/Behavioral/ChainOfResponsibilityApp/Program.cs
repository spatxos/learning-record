using System;

namespace ChainOfResponsibilityApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Chain of Responsibility Demo:\n");
        var manager = new Manager();
        var director = new Director();
        var ceo = new CEO();
        manager.SetNext(director);
        director.SetNext(ceo);

        int[] requests = { 500, 2000, 50000 };
        foreach (var r in requests) Console.WriteLine(manager.Handle(r));
    }
}
