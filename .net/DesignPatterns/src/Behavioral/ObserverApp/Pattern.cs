using System;
using System.Collections.Generic;

namespace ObserverApp;

public interface IObserver { void Update(string message); }

public class ConcreteObserver : IObserver
{
    private readonly string _name;
    public ConcreteObserver(string name) { _name = name; }
    public void Update(string message) => Console.WriteLine($"{_name} received: {message}");
}

public class Subject
{
    private readonly List<IObserver> _observers = new();
    public void Attach(IObserver o) => _observers.Add(o);
    public void Detach(IObserver o) => _observers.Remove(o);
    public void Notify(string msg) { foreach (var o in _observers) o.Update(msg); }
}
