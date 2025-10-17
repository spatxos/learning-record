namespace AdapterApp;

// Existing incompatible interface
public class Adaptee
{
    public string SpecificRequest() => "Adaptee: Specific behavior";
}

// Target interface the client expects
public interface ITarget
{
    string Request();
}

// Adapter makes Adaptee conform to ITarget
public class Adapter : ITarget
{
    private readonly Adaptee _adaptee = new Adaptee();
    public string Request() => _adaptee.SpecificRequest();
}
