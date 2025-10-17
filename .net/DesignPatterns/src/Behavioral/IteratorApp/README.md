模式：迭代器（Iterator）

模式简介：
迭代器模式提供一种方法顺序访问一个聚合对象中的元素，而不暴露其内部表示。通常通过实现 `IEnumerable` / `IEnumerator` 或自定义迭代器类。

关键类：
- `MyCollection`：实现 `IEnumerable<int>` 的集合示例
- `GetEnumerator()`：返回枚举器，用于遍历元素

运行示例：
```powershell
dotnet run --project src/Behavioral/IteratorApp/IteratorApp.csproj
```

适用场景 / 何时使用：
- 希望以统一方式遍历不同集合而不暴露其内部表示时使用。