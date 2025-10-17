using System;

namespace PrototypeApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Prototype Pattern Demo:\n");

        var original = new PersonPrototype("Alice", new Address("123 Main St"));
        Console.WriteLine("Original: " + original);

        var shallow = original.ShallowClone();
        var deep = original.DeepClone();

        Console.WriteLine("After cloning (before change):");
        Console.WriteLine("Shallow: " + shallow);
        Console.WriteLine("Deep: " + deep);

        Console.WriteLine();
        Console.WriteLine("Modify original address to '456 Oak Ave'");
        original.Address.Street = "456 Oak Ave";

        Console.WriteLine("After modifying original:");
        Console.WriteLine("Original: " + original);
        Console.WriteLine("Shallow: " + shallow + " (shares address)");
        Console.WriteLine("Deep: " + deep + " (independent)");
    }
}
