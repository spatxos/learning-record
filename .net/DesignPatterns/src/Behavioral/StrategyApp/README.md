模式：策略（Strategy）

模式简介：
策略模式定义一系列算法，将每个算法封装起来，并使它们可相互替换。策略让算法独立于使用它的客户而变化。

关键类：
- `IStrategy`：策略接口
- `AddStrategy` / `MultiplyStrategy`：具体策略实现
- `Context`：持有策略并在运行时切换或执行策略

运行示例：
```powershell
dotnet run --project src/Behavioral/StrategyApp/StrategyApp.csproj
```

适用场景 / 何时使用：
- 需要在运行时选择或切换算法，并希望将算法封装为可独立替换的组件时使用。