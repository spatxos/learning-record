namespace BuilderApp;

public class Product
{
    public string Parts { get; private set; } = string.Empty;
    public void Add(string part) => Parts = string.IsNullOrEmpty(Parts) ? part : Parts + ", " + part;
    public override string ToString() => Parts;
}

public interface IBuilder
{
    void Reset();
    void BuildPartA();
    void BuildPartB();
    Product GetResult();
}

public class ConcreteBuilder : IBuilder
{
    private Product _product = new Product();
    public void Reset() => _product = new Product();
    public void BuildPartA() => _product.Add("PartA");
    public void BuildPartB() => _product.Add("PartB");
    public Product GetResult() => _product;
}

public class Director
{
    public void BuildMinimal(IBuilder builder)
    {
        builder.Reset();
        builder.BuildPartA();
    }

    public void BuildFull(IBuilder builder)
    {
        builder.Reset();
        builder.BuildPartA();
        builder.BuildPartB();
    }
}
