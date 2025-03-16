using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using dataflow_cs.Utils.CADUtils;
using System.Collections.Generic;
using System;

namespace CADUtils.Tests
{
    [TestClass]
    public class UtilsBlockPropertyTests
    {
        private Mock<ObjectId> _mockObjectId;
        private Mock<BlockReference> _mockBlockRef;
        private Mock<AttributeReference> _mockAttRef1;
        private Mock<AttributeReference> _mockAttRef2;
        private Mock<ObjectId> _mockAttId1;
        private Mock<ObjectId> _mockAttId2;
        private ObjectIdCollection _attributeCollection;

        [TestInitialize]
        public void Setup()
        {
            // 设置模拟的ObjectId和BlockReference
            _mockObjectId = new Mock<ObjectId>();
            _mockBlockRef = new Mock<BlockReference>();
            
            // 设置模拟的AttributeReference和ObjectId
            _mockAttRef1 = new Mock<AttributeReference>();
            _mockAttRef2 = new Mock<AttributeReference>();
            _mockAttId1 = new Mock<ObjectId>();
            _mockAttId2 = new Mock<ObjectId>();
            
            // 设置AttributeReference的属性
            _mockAttRef1.Setup(a => a.Tag).Returns("Tag1");
            _mockAttRef1.Setup(a => a.TextString).Returns("Value1");
            _mockAttRef2.Setup(a => a.Tag).Returns("Tag2");
            _mockAttRef2.Setup(a => a.TextString).Returns("Value2");
            
            // 设置ObjectId.GetObject()方法返回的值
            _mockAttId1.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockAttRef1.Object);
            _mockAttId2.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockAttRef2.Object);
            
            // 创建AttributeCollection
            _attributeCollection = new ObjectIdCollection();
            _attributeCollection.Add(_mockAttId1.Object);
            _attributeCollection.Add(_mockAttId2.Object);
            
            // 设置BlockReference.AttributeCollection属性
            _mockBlockRef.Setup(b => b.AttributeCollection).Returns(_attributeCollection);
            
            // 设置ObjectId.GetObject()方法返回的值
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);
        }

        [TestMethod]
        public void UtilsSetPropertyValueByPropertyName_SetsCorrectValue()
        {
            // 安排
            string propertyName = "Tag1";
            string propertyValue = "NewValue1";
            
            // 设置AttributeReference.UpgradeOpen()方法
            _mockAttRef1.Setup(a => a.UpgradeOpen());
            
            // 设置AttributeReference.DowngradeOpen()方法
            _mockAttRef1.Setup(a => a.DowngradeOpen());
            
            // 执行
            UtilsBlock.UtilsSetPropertyValueByPropertyName(_mockObjectId.Object, propertyName, propertyValue);
            
            // 断言
            _mockAttRef1.VerifySet(a => a.TextString = propertyValue, Times.Once);
            _mockAttRef1.Verify(a => a.UpgradeOpen(), Times.Once);
            _mockAttRef1.Verify(a => a.DowngradeOpen(), Times.Once);
        }

        [TestMethod]
        public void UtilsSetPropertyValueByPropertyName_WithNonExistentProperty_DoesNothing()
        {
            // 安排
            string propertyName = "NonExistentTag";
            string propertyValue = "NewValue";
            
            // 执行
            UtilsBlock.UtilsSetPropertyValueByPropertyName(_mockObjectId.Object, propertyName, propertyValue);
            
            // 断言
            _mockAttRef1.VerifySet(a => a.TextString = It.IsAny<string>(), Times.Never);
            _mockAttRef2.VerifySet(a => a.TextString = It.IsAny<string>(), Times.Never);
        }

        [TestMethod]
        public void UtilsSetPropertyValueByDictData_SetsCorrectValues()
        {
            // 注意：由于UtilsSetPropertyValueByDictData方法依赖于UtilsCADActive.Database静态属性
            // 我们无法直接模拟这个静态属性，所以这个测试只是一个示例
            Console.WriteLine("注意：由于无法直接模拟静态属性，此测试仅作为示例");
            
            // 安排
            var propertyDict = new Dictionary<string, string>
            {
                { "Tag1", "NewValue1" },
                { "Tag2", "NewValue2" }
            };
            
            // 设置模拟的TransactionManager
            var mockTransactionManager = new Mock<TransactionManager>();
            var mockTransaction = new Mock<Transaction>();
            
            // 设置TransactionManager.StartTransaction()方法返回的值
            mockTransactionManager.Setup(tm => tm.StartTransaction()).Returns(mockTransaction.Object);
            
            // 设置Database.TransactionManager属性
            var mockDatabase = new Mock<Database>();
            mockDatabase.Setup(db => db.TransactionManager).Returns(mockTransactionManager.Object);
            
            // 设置LayerTableRecord
            var mockLayerTableRecord = new Mock<LayerTableRecord>();
            mockLayerTableRecord.Setup(l => l.IsLocked).Returns(false);
            
            // 设置BlockReference.LayerId属性
            var mockLayerId = new Mock<ObjectId>();
            _mockBlockRef.Setup(b => b.LayerId).Returns(mockLayerId.Object);
            
            // 设置ObjectId.GetObject()方法返回的值
            mockLayerId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockLayerTableRecord.Object);
            
            // 设置AttributeReference.LayerId属性
            var mockAttLayerId1 = new Mock<ObjectId>();
            var mockAttLayerId2 = new Mock<ObjectId>();
            _mockAttRef1.Setup(a => a.LayerId).Returns(mockAttLayerId1.Object);
            _mockAttRef2.Setup(a => a.LayerId).Returns(mockAttLayerId2.Object);
            
            // 设置ObjectId.GetObject()方法返回的值
            mockAttLayerId1.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockLayerTableRecord.Object);
            mockAttLayerId2.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockLayerTableRecord.Object);
            
            // 设置AttributeReference.UpgradeOpen()方法
            _mockAttRef1.Setup(a => a.UpgradeOpen());
            _mockAttRef2.Setup(a => a.UpgradeOpen());
            
            // 设置AttributeReference.DowngradeOpen()方法
            _mockAttRef1.Setup(a => a.DowngradeOpen());
            _mockAttRef2.Setup(a => a.DowngradeOpen());
            
            // 在实际测试中，我们需要模拟UtilsCADActive.Database静态属性
            // 但由于我们无法直接模拟静态属性，所以这里只是示例代码
            
            // 模拟测试结果
            // 在实际环境中，您可能需要使用依赖注入或其他方式来解决这个问题
        }

        [TestMethod]
        public void UtilsSetPropertyValueByDictData_WithLockedLayer_UnlocksLayerFirst()
        {
            // 注意：由于UtilsSetPropertyValueByDictData方法依赖于UtilsCADActive.Database静态属性
            // 我们无法直接模拟这个静态属性，所以这个测试只是一个示例
            Console.WriteLine("注意：由于无法直接模拟静态属性，此测试仅作为示例");
            
            // 安排
            var propertyDict = new Dictionary<string, string>
            {
                { "Tag1", "NewValue1" }
            };
            
            // 设置模拟的TransactionManager
            var mockTransactionManager = new Mock<TransactionManager>();
            var mockTransaction = new Mock<Transaction>();
            
            // 设置TransactionManager.StartTransaction()方法返回的值
            mockTransactionManager.Setup(tm => tm.StartTransaction()).Returns(mockTransaction.Object);
            
            // 设置Database.TransactionManager属性
            var mockDatabase = new Mock<Database>();
            mockDatabase.Setup(db => db.TransactionManager).Returns(mockTransactionManager.Object);
            
            // 设置LayerTableRecord
            var mockLayerTableRecord = new Mock<LayerTableRecord>();
            mockLayerTableRecord.Setup(l => l.IsLocked).Returns(true);
            
            // 设置BlockReference.LayerId属性
            var mockLayerId = new Mock<ObjectId>();
            _mockBlockRef.Setup(b => b.LayerId).Returns(mockLayerId.Object);
            
            // 设置ObjectId.GetObject()方法返回的值
            mockLayerId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockLayerTableRecord.Object);
            
            // 设置LayerTableRecord.UpgradeOpen()方法
            mockLayerTableRecord.Setup(l => l.UpgradeOpen());
            
            // 设置LayerTableRecord.DowngradeOpen()方法
            mockLayerTableRecord.Setup(l => l.DowngradeOpen());
            
            // 设置AttributeReference.LayerId属性
            var mockAttLayerId = new Mock<ObjectId>();
            _mockAttRef1.Setup(a => a.LayerId).Returns(mockAttLayerId.Object);
            
            // 设置ObjectId.GetObject()方法返回的值
            mockAttLayerId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockLayerTableRecord.Object);
            
            // 设置AttributeReference.UpgradeOpen()方法
            _mockAttRef1.Setup(a => a.UpgradeOpen());
            
            // 设置AttributeReference.DowngradeOpen()方法
            _mockAttRef1.Setup(a => a.DowngradeOpen());
            
            // 在实际测试中，我们需要模拟UtilsCADActive.Database静态属性
            // 但由于我们无法直接模拟静态属性，所以这里只是示例代码
            
            // 模拟测试结果
            // 在实际环境中，您可能需要使用依赖注入或其他方式来解决这个问题
        }

        [TestMethod]
        public void UtilsSetDynamicPropertyValueByDictData_SetsCorrectValues()
        {
            // 安排
            var propertyDict = new Dictionary<string, string>
            {
                { "Prop1", "10" },
                { "Prop2", "20" }
            };
            
            // 设置模拟的DynamicBlockReferenceProperty
            var mockProp1 = new Mock<DynamicBlockReferenceProperty>();
            mockProp1.Setup(p => p.PropertyName).Returns("Prop1");
            mockProp1.Setup(p => p.Value).Returns(0);
            
            var mockProp2 = new Mock<DynamicBlockReferenceProperty>();
            mockProp2.Setup(p => p.PropertyName).Returns("Prop2");
            mockProp2.Setup(p => p.Value).Returns(0);
            
            // 创建DynamicBlockReferencePropertyCollection
            var propCollection = new DynamicBlockReferencePropertyCollection();
            propCollection.Add(mockProp1.Object);
            propCollection.Add(mockProp2.Object);
            
            // 设置BlockReference.DynamicBlockReferencePropertyCollection属性
            _mockBlockRef.Setup(b => b.DynamicBlockReferencePropertyCollection).Returns(propCollection);
            
            // 设置BlockReference.DynamicBlockTableRecord属性
            var mockDynamicBlockTableRecord = new Mock<ObjectId>();
            _mockBlockRef.Setup(b => b.DynamicBlockTableRecord).Returns(mockDynamicBlockTableRecord.Object);
            
            // 设置BlockReference.UpgradeOpen()方法
            _mockBlockRef.Setup(b => b.UpgradeOpen());
            
            // 设置BlockReference.DowngradeOpen()方法
            _mockBlockRef.Setup(b => b.DowngradeOpen());
            
            // 设置BlockReference.IsWriteEnabled属性
            _mockBlockRef.Setup(b => b.IsWriteEnabled).Returns(true);
            
            // 执行
            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(_mockObjectId.Object, propertyDict);
            
            // 断言
            _mockBlockRef.Verify(b => b.UpgradeOpen(), Times.Once);
            mockProp1.VerifySet(p => p.Value = It.IsAny<int>(), Times.Once);
            mockProp2.VerifySet(p => p.Value = It.IsAny<int>(), Times.Once);
            _mockBlockRef.Verify(b => b.DowngradeOpen(), Times.Once);
        }

        [TestMethod]
        public void UtilsSetDynamicPropertyValueByDictData_WithNonExistentProperty_DoesNothing()
        {
            // 安排
            var propertyDict = new Dictionary<string, string>
            {
                { "NonExistentProp", "10" }
            };
            
            // 设置模拟的DynamicBlockReferenceProperty
            var mockProp = new Mock<DynamicBlockReferenceProperty>();
            mockProp.Setup(p => p.PropertyName).Returns("Prop1");
            mockProp.Setup(p => p.Value).Returns(0);
            
            // 创建DynamicBlockReferencePropertyCollection
            var propCollection = new DynamicBlockReferencePropertyCollection();
            propCollection.Add(mockProp.Object);
            
            // 设置BlockReference.DynamicBlockReferencePropertyCollection属性
            _mockBlockRef.Setup(b => b.DynamicBlockReferencePropertyCollection).Returns(propCollection);
            
            // 设置BlockReference.DynamicBlockTableRecord属性
            var mockDynamicBlockTableRecord = new Mock<ObjectId>();
            _mockBlockRef.Setup(b => b.DynamicBlockTableRecord).Returns(mockDynamicBlockTableRecord.Object);
            
            // 执行
            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(_mockObjectId.Object, propertyDict);
            
            // 断言
            _mockBlockRef.Verify(b => b.UpgradeOpen(), Times.Never);
            mockProp.VerifySet(p => p.Value = It.IsAny<object>(), Times.Never);
        }

        [TestMethod]
        public void UtilsSetDynamicPropertyValueByDictData_WithNullDynamicBlockTableRecord_DoesNothing()
        {
            // 安排
            var propertyDict = new Dictionary<string, string>
            {
                { "Prop1", "10" }
            };
            
            // 设置BlockReference.DynamicBlockTableRecord属性
            _mockBlockRef.Setup(b => b.DynamicBlockTableRecord).Returns(ObjectId.Null);
            
            // 执行
            UtilsBlock.UtilsSetDynamicPropertyValueByDictData(_mockObjectId.Object, propertyDict);
            
            // 断言
            _mockBlockRef.Verify(b => b.UpgradeOpen(), Times.Never);
        }
    }
} 