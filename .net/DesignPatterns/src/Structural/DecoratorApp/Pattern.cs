namespace DecoratorApp;

public interface IMessage
{
    string GetContent();
}

public class SimpleMessage : IMessage
{
    private readonly string _text;
    public SimpleMessage(string text) { _text = text; }
    public string GetContent() => _text;
}

public abstract class MessageDecorator : IMessage
{
    protected IMessage _inner;
    protected MessageDecorator(IMessage inner) { _inner = inner; }
    public abstract string GetContent();
}

public class EncryptedMessage : MessageDecorator
{
    public EncryptedMessage(IMessage inner) : base(inner) { }
    public override string GetContent() => "[Encrypted] " + _inner.GetContent();
}

public class CompressedMessage : MessageDecorator
{
    public CompressedMessage(IMessage inner) : base(inner) { }
    public override string GetContent() => "[Compressed] " + _inner.GetContent();
}
