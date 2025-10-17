模式：代理（Proxy）

模式简介：
代理模式为对象提供一个替代或占位符以控制对其访问。代理可以在访问目标对象前后做额外处理，如缓存、延迟加载、权限校验等。

关键类：
- `IService`：服务接口
- `RealService`：真实服务实现
- `CachingProxy`：代理实现，包装 `RealService` 并添加缓存逻辑

运行示例：
```powershell
dotnet run --project src/Structural/ProxyApp/ProxyApp.csproj
```

适用场景 / 何时使用：
- 需要在访问真实对象前后加入控制逻辑（如缓存、权限校验、延迟加载或远程代理）时使用。