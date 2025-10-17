using System;

namespace BuilderApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Builder Pattern Demo:\n");

        var director = new Director();
        var builder = new ConcreteBuilder();

        Console.WriteLine("Minimal product:");
        director.BuildMinimal(builder);
        Console.WriteLine(builder.GetResult());

        Console.WriteLine();
        Console.WriteLine("Full product:");
        director.BuildFull(builder);
        Console.WriteLine(builder.GetResult());

        Console.WriteLine();
        Console.WriteLine("Custom product (manual builder use):");
        builder.Reset();
        builder.BuildPartB();
        Console.WriteLine(builder.GetResult());
    }
}
