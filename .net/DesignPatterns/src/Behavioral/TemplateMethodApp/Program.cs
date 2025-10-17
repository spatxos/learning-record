using System;

namespace TemplateMethodApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Template Method Pattern Demo:\n");
        var c = new ConcreteClass();
        c.TemplateMethod();
    }
}
