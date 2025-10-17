namespace VisitorApp;

public interface IVisitor { void VisitConcreteA(ConcreteElementA a); void VisitConcreteB(ConcreteElementB b); }

public interface IElement { void Accept(IVisitor v); }

public class ConcreteElementA : IElement
{
    public void Accept(IVisitor v) => v.VisitConcreteA(this);
    public string OperationA() => "A operation";
}

public class ConcreteElementB : IElement
{
    public void Accept(IVisitor v) => v.VisitConcreteB(this);
    public string OperationB() => "B operation";
}

public class ConcreteVisitor : IVisitor
{
    public void VisitConcreteA(ConcreteElementA a) => System.Console.WriteLine("Visitor on " + a.OperationA());
    public void VisitConcreteB(ConcreteElementB b) => System.Console.WriteLine("Visitor on " + b.OperationB());
}
