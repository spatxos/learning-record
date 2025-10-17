模式：组合（Composite）

模式简介：
组合模式将对象组织成树形结构以表示“整体-部分”关系，使客户端对单个对象和组合对象具有一致的访问接口。常用于表示层次结构。

关键类：
- `Component`：抽象组件接口/基类
- `Leaf`：叶子节点，不能有子节点
- `Composite`：容器节点，持有并管理子组件

运行示例：
```powershell
dotnet run --project src/Structural/CompositeApp/CompositeApp.csproj
```

适用场景 / 何时使用：
- 表示树形结构（如文件系统、UI 组件），并希望对单个元素与组合元素使用相同接口时使用。