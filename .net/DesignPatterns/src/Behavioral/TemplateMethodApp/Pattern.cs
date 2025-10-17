namespace TemplateMethodApp;

public abstract class AbstractClass
{
    public void TemplateMethod()
    {
        Step1();
        Step2();
        Step3();
    }
    protected abstract void Step1();
    protected virtual void Step2() { }
    protected abstract void Step3();
}

public class ConcreteClass : AbstractClass
{
    protected override void Step1() => System.Console.WriteLine("Step1 implementation");
    protected override void Step3() => System.Console.WriteLine("Step3 implementation");
    protected override void Step2() => System.Console.WriteLine("Optional Step2 override");
}
