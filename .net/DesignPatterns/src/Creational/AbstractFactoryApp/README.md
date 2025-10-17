模式：抽象工厂（Abstract Factory）

模式简介：
抽象工厂用于创建一系列相关或相互依赖的对象，而无需指定它们的具体类。它通过定义用于创建对象的接口（工厂接口），并为每个产品族提供具体工厂实现，从而实现产品族间的解耦。

关键类：
- `IButton` / `ICheckbox`：产品接口
- `WinButton` / `WinCheckbox`、`LinuxButton` / `LinuxCheckbox`：具体产品
- `IGUIFactory`：抽象工厂接口
- `WinFactory` / `LinuxFactory`：具体工厂实现

运行示例：
在仓库根目录下运行：
```powershell
dotnet run --project src/Creational/AbstractFactoryApp/AbstractFactoryApp.csproj
```

适用场景 / 何时使用：
- 需要创建一系列相关对象并保证它们之间的兼容性；当需要在不同平台（或产品族）上切换整套对象实现时使用。