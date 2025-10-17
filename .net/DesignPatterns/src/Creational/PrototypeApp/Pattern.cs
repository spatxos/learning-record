namespace PrototypeApp;

public class Address
{
    public string Street { get; set; }
    public Address(string street) { Street = street; }
    public Address Clone() => new Address(Street);
    public override string ToString() => Street;
}

public class PersonPrototype
{
    public string Name { get; set; }
    public Address Address { get; set; }

    public PersonPrototype(string name, Address address)
    {
        Name = name;
        Address = address;
    }

    // Shallow clone
    public PersonPrototype ShallowClone() => (PersonPrototype)MemberwiseClone();

    // Deep clone
    public PersonPrototype DeepClone() => new PersonPrototype(Name, Address.Clone());

    public override string ToString() => $"{Name} @ {Address}";
}
