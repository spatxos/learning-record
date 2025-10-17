模式：命令（Command）

模式简介：
命令模式将请求封装为对象，从而使你可以用不同的请求对客户进行参数化、队列请求、记录日志或支持可撤销操作。典型角色有命令（Command）、接收者（Receiver）、调用者（Invoker）。

关键类：
- `ICommand`：命令接口，通常包含 `Execute()` 和 `Undo()`
- `Receiver`：执行实际操作的类
- `AddCommand`：具体命令实现
- `Invoker`：持有命令并触发执行/撤销

运行示例：
```powershell
dotnet run --project src/Behavioral/CommandApp/CommandApp.csproj
```

适用场景 / 何时使用：
- 需要将操作封装为对象以便支持队列、撤销/重做、日志记录或远程执行时使用。