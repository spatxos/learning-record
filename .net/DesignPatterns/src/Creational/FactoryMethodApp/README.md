模式：工厂方法（Factory Method）

模式简介：
工厂方法定义一个创建对象的接口，但由子类决定要实例化的类。它使一个类的实例化延迟到子类进行，从而将创建逻辑封装在子类中。

关键类：
- `Creator`（抽象创建者）
- `ConcreteCreator`（具体创建者）
- `Product`（产品接口）
- `ConcreteProduct`（具体产品）

运行示例：
```powershell
dotnet run --project src/Creational/FactoryMethodApp/FactoryMethodApp.csproj
```
 
适用场景 / 何时使用：
- 当类不知道它必须创建哪些对象，或者想把子类作为创建者以便扩展新产品时使用。