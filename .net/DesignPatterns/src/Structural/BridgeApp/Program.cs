using System;

namespace BridgeApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Bridge Pattern Demo:\n");
        var vectorCircle = new Circle(new VectorRenderer(), 5);
        var rasterCircle = new Circle(new RasterRenderer(), 5);
        Console.WriteLine(vectorCircle.Draw());
        Console.WriteLine(rasterCircle.Draw());
    }
}
