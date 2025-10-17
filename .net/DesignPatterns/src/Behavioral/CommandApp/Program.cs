using System;

namespace CommandApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Command Pattern Demo:\n");
        var receiver = new Receiver();
        var invoker = new Invoker();
        invoker.ExecuteCommand(new AddCommand(receiver, 5));
        Console.WriteLine($"State after add 5: {receiver.State}");
        invoker.ExecuteCommand(new AddCommand(receiver, 3));
        Console.WriteLine($"State after add 3: {receiver.State}");
        invoker.Undo();
        Console.WriteLine($"State after undo: {receiver.State}");
    }
}
