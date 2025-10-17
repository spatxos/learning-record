模式：观察者（Observer）

模式简介：
观察者模式定义对象间一对多的依赖关系，当一个对象状态发生改变时，所有依赖它的对象都会得到通知并自动更新。通常包含 Subject（主题）和 Observer（观察者）。

关键类：
- `IObserver` / `ConcreteObserver`：观察者接口与实现
- `Subject`：维护观察者列表并在状态变化时通知

运行示例：
```powershell
dotnet run --project src/Behavioral/ObserverApp/ObserverApp.csproj
```

适用场景 / 何时使用：
- 系统中存在一对多依赖关系，主题状态变化需要自动通知多个观察者（如事件分发、UI 数据绑定）时使用。