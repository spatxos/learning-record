using System.Collections.Generic;

namespace CompositeApp;

public abstract class Component
{
    public string Name { get; }
    protected Component(string name) { Name = name; }
    public abstract void Print(string indent = "");
}

public class Leaf : Component
{
    public Leaf(string name) : base(name) { }
    public override void Print(string indent = "") => System.Console.WriteLine(indent + Name);
}

public class Composite : Component
{
    private readonly List<Component> _children = new();
    public Composite(string name) : base(name) { }
    public void Add(Component c) => _children.Add(c);
    public override void Print(string indent = "")
    {
        System.Console.WriteLine(indent + Name);
        foreach (var c in _children) c.Print(indent + "  ");
    }
}
