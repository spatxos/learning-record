using System;

namespace IteratorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Iterator Pattern Demo:\n");
        var c = new MyCollection();
        c.Add(1); c.Add(2); c.Add(3);
        foreach (var i in c) Console.WriteLine(i);
    }
}
