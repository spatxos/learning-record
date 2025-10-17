namespace AbstractFactoryApp;

// Abstract product A
public interface IButton
{
    string Render();
}

// Abstract product B
public interface ICheckbox
{
    string Render();
}

// Concrete products for Windows
public class WinButton : IButton { public string Render() => "Rendering Windows Button"; }
public class WinCheckbox : ICheckbox { public string Render() => "Rendering Windows Checkbox"; }

// Concrete products for Linux
public class LinuxButton : IButton { public string Render() => "Rendering Linux Button"; }
public class LinuxCheckbox : ICheckbox { public string Render() => "Rendering Linux Checkbox"; }

// Abstract factory
public interface IGUIFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}

// Concrete factories
public class WinFactory : IGUIFactory
{
    public IButton CreateButton() => new WinButton();
    public ICheckbox CreateCheckbox() => new WinCheckbox();
}

public class LinuxFactory : IGUIFactory
{
    public IButton CreateButton() => new LinuxButton();
    public ICheckbox CreateCheckbox() => new LinuxCheckbox();
}
