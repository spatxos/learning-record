模式：桥接（Bridge）

模式简介：
桥接模式将抽象部分与实现部分分离，使它们可以独立变化。它使用组合（而不是继承）将抽象与实现解耦，适合在多个维度上扩展时使用。

关键类：
- `IRenderer`：实现层接口
- `VectorRenderer` / `RasterRenderer`：具体实现
- `Shape`：抽象部分，使用 `IRenderer`
- `Circle`：具体抽象实现

运行示例：
```powershell
dotnet run --project src/Structural/BridgeApp/BridgeApp.csproj
```

适用场景 / 何时使用：
- 抽象和实现需要独立扩展，且希望避免为每种组合都写子类时使用。