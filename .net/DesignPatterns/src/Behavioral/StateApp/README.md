模式：状态（State）

模式简介：
状态模式允许对象在其内部状态改变时改变其行为，看起来像改变了其类。常通过把状态封装成独立的状态类并在上下文中切换来实现。

关键类：
- `IState`：状态接口
- `ConcreteStateA` / `ConcreteStateB`：具体状态实现
- `Context`：维护当前状态并委托行为给状态对象

运行示例：
```powershell
dotnet run --project src/Behavioral/StateApp/StateApp.csproj
```

适用场景 / 何时使用：
- 当对象的行为依赖于其内部状态且状态之间切换频繁，使用状态模式可避免庞大的条件分支并把状态封装成对象。