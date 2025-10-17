using System;

namespace CompositeApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Composite Pattern Demo:\n");
        var root = new Composite("root");
        var child1 = new Composite("child1");
        child1.Add(new Leaf("leaf1"));
        child1.Add(new Leaf("leaf2"));
        var child2 = new Composite("child2");
        child2.Add(new Leaf("leaf3"));
        root.Add(child1);
        root.Add(child2);
        root.Print();
    }
}
