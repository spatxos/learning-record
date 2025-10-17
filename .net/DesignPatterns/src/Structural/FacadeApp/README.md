模式：外观（Facade）

模式简介：
外观模式为一组接口提供一个统一的高层接口，使得子系统更容易使用。它把复杂的子系统调用封装在单一的外观类中，简化客户端交互。

关键类：
- `SubsystemA` / `SubsystemB` / `SubsystemC`：子系统类
- `Facade`：封装子系统并提供简化接口给客户端

运行示例：
```powershell
dotnet run --project src/Structural/FacadeApp/FacadeApp.csproj
```

适用场景 / 何时使用：
- 当需要为复杂子系统提供简化统一接口，降低客户端与子系统的耦合度时使用。