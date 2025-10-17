using System;

namespace ProxyApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Proxy Pattern Demo:\n");
        IService service = new CachingProxy();
        Console.WriteLine(service.GetData("a"));
        Console.WriteLine(service.GetData("a"));
        Console.WriteLine(service.GetData("b"));
    }
}
