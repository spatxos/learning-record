模式：访问者（Visitor）

模式简介：
访问者模式表示一个作用于某对象结构中的各元素的操作，它允许你在不改变元素类的前提下定义新的操作。通常涉及 Visitor（访问者）和 Element（元素）接口。

关键类：
- `IVisitor` / `ConcreteVisitor`：访问者接口与实现
- `IElement` / `ConcreteElementA` / `ConcreteElementB`：元素接口与具体实现

运行示例：
```powershell
dotnet run --project src/Behavioral/VisitorApp/VisitorApp.csproj
```

适用场景 / 何时使用：
- 需要对对象结构中的元素执行多种不相关的操作，并希望把这些操作从数据结构中分离出来时使用（如编译器对 AST 的各种遍历处理）。