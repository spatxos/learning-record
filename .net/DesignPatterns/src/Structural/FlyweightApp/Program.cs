using System;

namespace FlyweightApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Flyweight Pattern Demo:\n");
        var factory = new FlyweightFactory();
        var a1 = factory.Get('a');
        var a2 = factory.Get('a');
        Console.WriteLine(a1.Display(12));
        Console.WriteLine(a2.Display(14));
        Console.WriteLine($"Same instance: {ReferenceEquals(a1, a2)}");
    }
}
