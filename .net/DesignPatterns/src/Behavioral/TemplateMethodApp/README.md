模式：模板方法（Template Method）

模式简介：
模板方法在父类中定义一个算法的骨架，而将某些步骤延迟到子类中实现。子类可以在不改变算法结构的情况下重新定义这些步骤。

关键类：
- `AbstractClass`：包含模板方法并定义基本步骤（部分可抽象）
- `ConcreteClass`：实现抽象步骤以完成具体算法

运行示例：
```powershell
dotnet run --project src/Behavioral/TemplateMethodApp/TemplateMethodApp.csproj
```

适用场景 / 何时使用：
- 当多个类共享同一算法的骨架但某些步骤需要由子类实现时使用。