namespace BridgeApp;

// Implementor
public interface IRenderer
{
    string RenderCircle(float radius);
}

public class VectorRenderer : IRenderer
{
    public string RenderCircle(float radius) => $"Vector circle of radius {radius}";
}

public class RasterRenderer : IRenderer
{
    public string RenderCircle(float radius) => $"Rasterizing circle of radius {radius}";
}

// Abstraction
public abstract class Shape
{
    protected IRenderer Renderer;
    protected Shape(IRenderer renderer) { Renderer = renderer; }
    public abstract string Draw();
}

public class Circle : Shape
{
    public float Radius { get; set; }
    public Circle(IRenderer renderer, float radius) : base(renderer) { Radius = radius; }
    public override string Draw() => Renderer.RenderCircle(Radius);
}
