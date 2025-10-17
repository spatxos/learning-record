模式：单例（Singleton）

模式简介：
单例模式确保一个类只有一个实例，并提供全局访问点。常见实现包括懒汉式、饿汉式和双重检查锁定（double-checked locking）。

关键类：
- `Singleton`：包含私有构造函数和静态实例访问方法（`Instance`）

运行示例：
```powershell
dotnet run --project src/Creational/SingletonApp/SingletonApp.csproj
```

适用场景 / 何时使用：
- 需要全局唯一实例（例如配置、日志、连接池），并且需要集中控制其创建与访问时使用。