using System;

namespace AdapterApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Adapter Pattern Demo:\n");
        ITarget adapter = new Adapter();
        Console.WriteLine(adapter.Request());
    }
}
