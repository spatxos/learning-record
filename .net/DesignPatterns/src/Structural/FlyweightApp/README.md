模式：享元（Flyweight）

模式简介：
享元模式通过共享相同的不可变对象来减少内存占用，适用于大量相似对象的场景。将对象状态分为可共享的内部状态和不可共享的外部状态。

关键类：
- `CharacterFlyweight`：享元对象，包含可共享的内部状态
- `FlyweightFactory`：管理并复用享元实例

运行示例：
```powershell
dotnet run --project src/Structural/FlyweightApp/FlyweightApp.csproj
```

适用场景 / 何时使用：
- 当应用中有大量相似对象且可共享内部状态以节省内存（如字符/格式化对象池）时使用。