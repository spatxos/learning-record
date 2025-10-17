namespace SingletonApp;

public sealed class Singleton
{
    private static readonly object _lock = new object();
    private static Singleton? _instance;
    public int Value { get; private set; }

    private Singleton(int value) { Value = value; }

    public static Singleton GetInstance(int value)
    {
        if (_instance is null)
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = new Singleton(value);
                }
            }
        }
        return _instance;
    }
}
