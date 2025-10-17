模式：原型（Prototype）

模式简介：
原型模式通过复制已有实例来创建新对象，从而避免通过构造函数进行复杂初始化。典型方法是实现 `Clone` 或使用 `MemberwiseClone` 来完成浅拷贝/深拷贝。

关键类：
- `PersonPrototype` / `Address`：示例原型类，展示浅拷贝与深拷贝
- `Clone` 方法：用于复制对象

运行示例：
```powershell
dotnet run --project src/Creational/PrototypeApp/PrototypeApp.csproj
```

适用场景 / 何时使用：
- 当创建对象成本较高或构建复杂，且可以通过复制已有对象来快速生成新实例时使用。