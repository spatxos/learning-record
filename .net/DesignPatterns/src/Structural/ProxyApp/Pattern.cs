using System.Collections.Generic;

namespace ProxyApp;

public interface IService
{
    string GetData(string key);
}

public class RealService : IService
{
    public string GetData(string key) => $"Data for {key} (from real service)";
}

public class CachingProxy : IService
{
    private readonly RealService _real = new();
    private readonly Dictionary<string, string> _cache = new();
    public string GetData(string key)
    {
        if (_cache.ContainsKey(key)) return _cache[key] + " (from cache)";
        var data = _real.GetData(key);
        _cache[key] = data;
        return data;
    }
}
