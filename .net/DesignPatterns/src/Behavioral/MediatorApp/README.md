模式：中介者（Mediator）

模式简介：
中介者模式用一个中介对象来封装一系列对象之间的交互，从而使各对象不需要显式引用彼此，降低耦合度。所有交互通过中介者转发或协调。

关键类：
- `IMediator` / `ConcreteMediator`：中介者接口与实现
- `Component1` / `Component2`：同事类，通过中介者通信

运行示例：
```powershell
dotnet run --project src/Behavioral/MediatorApp/MediatorApp.csproj
```

适用场景 / 何时使用：
- 当多个组件之间存在复杂交互且希望把交互逻辑集中在一个地方以降低耦合时使用。