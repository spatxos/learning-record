using System;

namespace FacadeApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Facade Pattern Demo:\n");
        var facade = new Facade();
        Console.WriteLine(facade.Operation());
    }
}
