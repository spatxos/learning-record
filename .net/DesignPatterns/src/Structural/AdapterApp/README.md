模式：适配器（Adapter）

模式简介：
适配器模式用于将一个类的接口转换成客户期望的另一个接口，使原本接口不兼容的类能够一起工作。常用于遗留代码或第三方库的包装。

关键类：
- `Adaptee`：已有的不兼容接口
- `ITarget`：目标接口（客户端期望）
- `Adapter`：实现 `ITarget` 并内部持有 `Adaptee`，执行适配转换

运行示例：
```powershell
dotnet run --project src/Structural/AdapterApp/AdapterApp.csproj
```

适用场景 / 何时使用：
- 需要将已有类/库的接口适配为客户端期望的接口，或将遗留系统与新系统集成时使用。