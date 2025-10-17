模式：责任链（Chain of Responsibility）

模式简介：
责任链模式让多个对象都有机会处理请求，将这些对象连成一条链，并沿着这条链传递请求，直到有对象处理它。这样将发送者和接收者解耦。

关键类：
- `Handler`：抽象处理者，持有指向下一个处理者的引用
- `Manager` / `Director` / `CEO`：具体处理者，根据条件决定是否处理或传递

运行示例：
```powershell
dotnet run --project src/Behavioral/ChainOfResponsibilityApp/ChainOfResponsibilityApp.csproj
```

适用场景 / 何时使用：
- 当多个对象都有可能处理同一请求，且希望动态构建处理链或避免请求发送者与接收者耦合时使用。