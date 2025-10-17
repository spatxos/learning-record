模式：备忘录（Memento）

模式简介：
备忘录模式在不破坏封装性的前提下，捕获并外部保存一个对象的内部状态，以便以后恢复。通常涉及 Originator（发起人）、Memento（备忘录）和 Caretaker（管理者）。

关键类：
- `Memento`：保存状态的只读对象
- `Originator`：创建和恢复备忘录
- `Caretaker`：保存备忘录（历史）并触发恢复

运行示例：
```powershell
dotnet run --project src/Behavioral/MementoApp/MementoApp.csproj
```

适用场景 / 何时使用：
- 需要保存和恢复对象状态（如实现撤销/恢复功能）且不希望暴露对象内部表示时使用。