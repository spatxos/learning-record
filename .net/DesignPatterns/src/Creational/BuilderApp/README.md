模式：建造者（Builder）

模式简介：
建造者模式将一个复杂对象的构建与表示分离，使同样的构建过程可以创建不同的表示。通常包含 Director（指挥者）、Builder（抽象建造者）和 ConcreteBuilder（具体建造者）。

关键类：
- `Product`：最终构建的产品
- `IBuilder`：建造者接口，定义构建步骤
- `ConcreteBuilder`：具体建造者，实现步骤并返回产品
- `Director`：指挥构建顺序以生成不同配置

运行示例：
```powershell
dotnet run --project src/Creational/BuilderApp/BuilderApp.csproj
```

适用场景 / 何时使用：
- 构建复杂对象需要多个步骤或可选部件时；当希望将构建算法和表示分离以复用构建过程时使用。