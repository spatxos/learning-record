using System;

namespace ObserverApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Observer Pattern Demo:\n");
        var subject = new Subject();
        var o1 = new ConcreteObserver("Observer1");
        var o2 = new ConcreteObserver("Observer2");
        subject.Attach(o1);
        subject.Attach(o2);
        subject.Notify("Event 1");
        subject.Detach(o1);
        subject.Notify("Event 2");
    }
}
