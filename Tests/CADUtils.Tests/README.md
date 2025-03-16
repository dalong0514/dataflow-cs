# CADUtils 单元测试

本项目包含对 `dataflow-cs.Utils.CADUtils.UtilsBlock` 类的单元测试。

## 测试结构

测试分为三个主要类：

1. `UtilsBlockTests` - 测试基本的块属性和方法
2. `UtilsBlockSelectionTests` - 测试与选择集相关的方法
3. `UtilsBlockPropertyTests` - 测试与属性设置相关的方法

## 运行测试

要运行测试，请按照以下步骤操作：

1. 确保已安装 .NET SDK 8.0 或更高版本
2. 打开命令行终端，导航到测试项目目录：
   ```
   cd D:\dataflow.code\dataflow-cs\Tests\CADUtils.Tests
   ```
3. 运行测试：
   ```
   dotnet test
   ```
   
   或者直接运行控制台应用程序：
   ```
   dotnet run
   ```

## 注意事项

- 这些测试使用 Moq 框架来模拟 AutoCAD 对象，因此不需要实际的 AutoCAD 环境
- 由于无法直接模拟静态方法和属性，某些测试（如依赖于 `UtilsCADActive.Database` 的测试）仅作为示例，不会实际执行
- 在实际项目中，您可能需要使用依赖注入或其他方式来解决静态方法和属性的模拟问题

## 测试覆盖的方法

### UtilsBlockTests

- `UtilsGetBlockBasePoint`
- `UtilsChangeBlockLayerName`
- `UtilsGetBlockName`
- `UtilsGetBlockLayer`
- `UtilsGetAllPropertyDictList`
- `UtilsGetPropertyDictListByPropertyNameList`
- `UtilsGetPropertyValueByPropertyName`
- `UtilsGetBlockRotaton`
- `UtilsGetBlockRotatonInDegrees`
- `UtilsSetBlockRotaton`
- `UtilsSetBlockRotatonInDegrees`
- `UtilsSetBlockXYScale`

### UtilsBlockSelectionTests

- `UtilsGetObjectIdsBySelectByBlockName` (示例)
- `UtilsGetAllBlockObjectIds` (示例)
- `UtilsGetAllObjectIdsByBlockName` (示例)
- `UtilsGetAllObjectIdsByBlockName` (重载版本，接受 blockIds 参数)
- `UtilsGetAllObjectIdsGroupsByBlockNameList`
- `UtilsGetAllObjectIdsByBlockNameByCrossingWindow` (示例)

### UtilsBlockPropertyTests

- `UtilsSetPropertyValueByPropertyName`
- `UtilsSetPropertyValueByDictData` (示例)
- `UtilsSetDynamicPropertyValueByDictData` 

## 自定义测试运行器

项目包含一个自定义的测试运行器 (`Program.cs`)，它使用反射来查找和执行所有标记为 `[TestClass]` 和 `[TestMethod]` 的测试。这使得测试可以在没有 Visual Studio 或其他测试运行器的情况下运行。 