模式：装饰器（Decorator）

模式简介：
装饰器模式在不改变对象自身的情况下，动态地给对象添加职责。通过创建一系列装饰类包裹原始对象，扩展功能而不修改原类。

关键类：
- `IMessage`：组件接口
- `SimpleMessage`：具体组件
- `MessageDecorator`：抽象装饰器，持有 `IMessage` 引用
- `EncryptedMessage` / `CompressedMessage`：具体装饰器实现
运行示例：
```powershell
dotnet run --project src/Structural/DecoratorApp/DecoratorApp.csproj
```

适用场景 / 何时使用：
- 在不修改原类的情况下动态地给对象添加职责或行为；适合需要灵活组合功能的场景。