using System;

namespace DecoratorApp;

class Program
{
    static void Main()
    {
        Console.WriteLine("Decorator Pattern Demo:\n");
        IMessage message = new SimpleMessage("Hello World");
        message = new EncryptedMessage(message);
        message = new CompressedMessage(message);
        Console.WriteLine(message.GetContent());
    }
}
