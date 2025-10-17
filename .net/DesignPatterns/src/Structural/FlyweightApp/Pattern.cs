using System.Collections.Generic;

namespace FlyweightApp;

public class CharacterFlyweight
{
    public char Character { get; }
    public CharacterFlyweight(char c) { Character = c; }
    public string Display(int extrinsicSize) => $"{Character} (size {extrinsicSize})";
}

public class FlyweightFactory
{
    private readonly Dictionary<char, CharacterFlyweight> _pool = new();
    public CharacterFlyweight Get(char c)
    {
        if (!_pool.ContainsKey(c)) _pool[c] = new CharacterFlyweight(c);
        return _pool[c];
    }
}
