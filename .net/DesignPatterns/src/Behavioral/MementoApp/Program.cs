using System;

namespace MementoApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Memento Pattern Demo:\n");
        var origin = new Originator();
        var caretaker = new Caretaker();
        origin.State = "State1"; caretaker.Add(origin.Save());
        origin.State = "State2"; caretaker.Add(origin.Save());
        origin.State = "State3";
        Console.WriteLine("Current: " + origin.State);
        origin.Restore(caretaker.Get(0));
        Console.WriteLine("Restored: " + origin.State);
    }
}
