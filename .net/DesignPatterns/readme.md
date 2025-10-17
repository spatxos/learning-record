Creational patterns  ������ģʽ
    �������� Factory Method
    ���󹤳� Abstract Factory
    ������ Builder
    ԭ�� Prototype
    ���� Singleton
Structural patterns  �ṹ��ģʽ
    ������ Adapter
    # 设计模式示例 (Design Patterns)

    本仓库按类别提供了多个设计模式的最小可运行示例（每个示例为单独的 .NET 控制台项目），目录结构如下：

    Creational patterns (创建型)
     - FactoryMethodApp (已存在示例)
     - AbstractFactoryApp
     - BuilderApp
     - PrototypeApp
     - SingletonApp

    Structural patterns (结构型)
     - AdapterApp
     - BridgeApp
     - CompositeApp
     - DecoratorApp
     - FacadeApp
     - FlyweightApp
     - ProxyApp

    Behavioral patterns (行为型)
     - ChainOfResponsibilityApp
     - CommandApp
     - IteratorApp
     - MediatorApp
     - MementoApp
     - ObserverApp
     - StateApp
     - StrategyApp
     - TemplateMethodApp
     - VisitorApp

    运行说明
     - 在 PowerShell 或命令行中，可以对某个示例执行：

    ```powershell
    dotnet run --project src/Creational/AbstractFactoryApp/AbstractFactoryApp.csproj
    dotnet run --project src/Structural/AdapterApp/AdapterApp.csproj
    dotnet run --project src/Behavioral/ObserverApp/ObserverApp.csproj
    ```

    示例：在仓库根目录下运行全部项目的快速方法（按需选择）：

    ```powershell
    dotnet build DesignPatterns.sln -c Debug
    # 然后运行你想要的某个示例，例如：
    dotnet run --project src/Behavioral/CommandApp/CommandApp.csproj
    ```

    后续建议
     - 可在每个项目目录下补充更详细的注释或 demo 数据，以便教学演示。
     - 我可以把每个项目添加简短 README、或创建一个统一的演示脚本，按你偏好继续完善。

    ### 设计模型对比（快速表格）

    | 模型类别 | 关注点 | 目标 | 典型使用场景 |
    |---|---|---|---|
    | 创建型（Creational） | 对象创建 | 解耦实例化、提供可配置的创建方式 | 延迟实例化、选择不同产品实现、构建复杂对象、控制单例 |
    | 结构型（Structural） | 类/对象的组合与关系 | 通过组合/包装/代理等简化接口或复用代码 | 适配不兼容接口、在不改动类的前提下扩展功能、构建树形结构、节省内存 |
    | 行为型（Behavioral） | 对象间通信与职责分配 | 封装变化的行为，简化交互和控制流 | 封装可替换算法、集中管理请求处理、管理状态与交互 |

    ### 各设计模式简要适用场景

    （下面为每个模式给出“何时使用”的一句话说明，详细示例见各项目 README）

    | 模式 | 何时使用 |
    |---|---|
    | Factory Method | 当类不能预先决定要创建的具体产品类，且希望把创建职责交给子类 |
    | Abstract Factory | 当需要创建相关或相互依赖的一系列对象且保证产品族一致性 |
    | Builder | 当构建复杂对象需要分步骤并要支持不同表示时 |
    | Prototype | 当创建对象代价高或需要通过复制已有实例来快速生成对象时 |
    | Singleton | 当需要全局唯一实例并集中管理资源时 |
    | Adapter | 当需要把已有接口适配成客户端期望接口时 |
    | Bridge | 当抽象与实现需要独立扩展以避免类爆炸时 |
    | Composite | 当需要以统一接口处理单个对象与组合对象（树形结构）时 |
    | Decorator | 当需要在运行时给对象动态添加职责时 |
    | Facade | 当要为复杂子系统提供简化统一接口时 |
    | Flyweight | 当需创建大量相似对象且要共享内部状态以节省内存时 |
    | Proxy | 当需在访问对象前后插入控制（缓存、权限、延迟等）时 |
    | Chain of Responsibility | 当多个对象可能处理请求且希望动态决定处理者时 |
    | Command | 当需将请求封装为对象以支持队列、撤销或宏操作时 |
    | Iterator | 当需以统一方式遍历集合且不暴露内部表示时 |
    | Mediator | 当多对象交互复杂且需中央化协调以降低耦合时 |
    | Memento | 当需保存与恢复对象状态（如撤销功能）且不泄露内部表示时 |
    | Observer | 当对象状态改变需通知多个依赖对象时 |
    | State | 当对象行为随状态改变且状态切换频繁时 |
    | Strategy | 当需在运行时选择不同算法并封装算法时 |
    | Template Method | 当算法骨架固定但部分步骤需由子类实现时 |
    | Visitor | 当需在不改动数据结构下对元素执行多种操作时 |
