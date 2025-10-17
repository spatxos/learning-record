using System;

namespace AbstractFactoryApp;

class Program
{
    static void RunDemo(IGUIFactory factory)
    {
        var button = factory.CreateButton();
        var checkbox = factory.CreateCheckbox();
        Console.WriteLine(button.Render());
        Console.WriteLine(checkbox.Render());
    }

    static void Main()
    {
        Console.WriteLine("Abstract Factory Demo:\n");
        Console.WriteLine("Using Windows factory:");
        RunDemo(new WinFactory());

        Console.WriteLine();
        Console.WriteLine("Using Linux factory:");
        RunDemo(new LinuxFactory());
    }
}
