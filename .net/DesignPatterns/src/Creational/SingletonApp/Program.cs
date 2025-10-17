using System;

namespace SingletonApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Singleton Pattern Demo:\n");

        var s1 = Singleton.GetInstance(10);
        Console.WriteLine($"First instance value: {s1.Value}");

        var s2 = Singleton.GetInstance(20);
        Console.WriteLine($"Second instance value (should be same as first): {s2.Value}");

        Console.WriteLine($"Reference equal: {ReferenceEquals(s1, s2)}");
    }
}
