using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using dataflow_cs.Utils.CADUtils;
using System.Collections.Generic;
using System;

namespace CADUtils.Tests
{
    [TestClass]
    public class UtilsBlockTests
    {
        private Mock<ObjectId> _mockObjectId;
        private Mock<BlockReference> _mockBlockRef;
        private Mock<BlockTableRecord> _mockBlockTableRecord;

        [TestInitialize]
        public void Setup()
        {
            _mockObjectId = new Mock<ObjectId>();
            _mockBlockRef = new Mock<BlockReference>();
            _mockBlockTableRecord = new Mock<BlockTableRecord>();
        }

        [TestMethod]
        public void UtilsGetBlockBasePoint_ReturnsCorrectPosition()
        {
            // Arrange
            var expectedPosition = new Point3d(10, 20, 0);
            _mockBlockRef.Setup(b => b.Position).Returns(expectedPosition);
            
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockBasePoint(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(expectedPosition, result);
        }

        [TestMethod]
        public void UtilsChangeBlockLayerName_SetsCorrectLayerName()
        {
            // Arrange
            string expectedLayerName = "NewLayer";
            
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForWrite)).Returns(_mockBlockRef.Object);

            // Act
            UtilsBlock.UtilsChangeBlockLayerName(_mockObjectId.Object, expectedLayerName);

            // Assert
            _mockBlockRef.VerifySet(b => b.Layer = expectedLayerName, Times.Once);
        }

        [TestMethod]
        public void UtilsGetBlockName_ReturnsCorrectName()
        {
            // Arrange
            string expectedName = "BlockName";
            var blockId = new ObjectId();
            
            _mockBlockRef.Setup(b => b.IsDynamicBlock).Returns(false);
            _mockBlockRef.Setup(b => b.BlockTableRecord).Returns(blockId);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);
            
            // Mock the BlockTableRecord
            _mockBlockTableRecord.Setup(b => b.Name).Returns(expectedName);
            
            // Setup for blockId.GetObject
            Mock<ObjectId> mockBlockId = new Mock<ObjectId>();
            mockBlockId.Setup(o => o.IsValid).Returns(true);
            mockBlockId.Setup(o => o.IsErased).Returns(false);
            mockBlockId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockTableRecord.Object);
            
            // Replace the real blockId with our mock
            _mockBlockRef.Setup(b => b.BlockTableRecord).Returns(mockBlockId.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockName(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(expectedName, result);
        }

        [TestMethod]
        public void UtilsGetBlockName_WithNullBlockRef_ReturnsEmptyString()
        {
            // Arrange
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns((BlockReference)null);

            // Act
            var result = UtilsBlock.UtilsGetBlockName(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void UtilsGetBlockName_WithInvalidBlockId_ReturnsEmptyString()
        {
            // Arrange
            Mock<ObjectId> mockBlockId = new Mock<ObjectId>();
            mockBlockId.Setup(o => o.IsValid).Returns(false);
            
            _mockBlockRef.Setup(b => b.IsDynamicBlock).Returns(false);
            _mockBlockRef.Setup(b => b.BlockTableRecord).Returns(mockBlockId.Object);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockName(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void UtilsGetBlockLayer_ReturnsCorrectLayer()
        {
            // Arrange
            string expectedLayer = "Layer1";
            
            _mockBlockRef.Setup(b => b.Layer).Returns(expectedLayer);
            _mockBlockRef.Setup(b => b.IsDynamicBlock).Returns(false);
            
            Mock<ObjectId> mockBlockId = new Mock<ObjectId>();
            mockBlockId.Setup(o => o.IsValid).Returns(true);
            mockBlockId.Setup(o => o.IsErased).Returns(false);
            
            _mockBlockRef.Setup(b => b.BlockTableRecord).Returns(mockBlockId.Object);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockLayer(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(expectedLayer, result);
        }

        [TestMethod]
        public void UtilsGetBlockLayer_WithNullBlockRef_ReturnsEmptyString()
        {
            // Arrange
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns((BlockReference)null);

            // Act
            var result = UtilsBlock.UtilsGetBlockLayer(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void UtilsGetAllPropertyDictList_ReturnsCorrectDictionary()
        {
            // Arrange
            var attributeCollection = new ObjectIdCollection();
            var mockAttId1 = new Mock<ObjectId>();
            var mockAttId2 = new Mock<ObjectId>();
            
            var mockAttRef1 = new Mock<AttributeReference>();
            mockAttRef1.Setup(a => a.Tag).Returns("Tag1");
            mockAttRef1.Setup(a => a.TextString).Returns("Value1");
            
            var mockAttRef2 = new Mock<AttributeReference>();
            mockAttRef2.Setup(a => a.Tag).Returns("Tag2");
            mockAttRef2.Setup(a => a.TextString).Returns("Value2");
            
            mockAttId1.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef1.Object);
            mockAttId2.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef2.Object);
            
            attributeCollection.Add(mockAttId1.Object);
            attributeCollection.Add(mockAttId2.Object);
            
            _mockBlockRef.Setup(b => b.AttributeCollection).Returns(attributeCollection);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetAllPropertyDictList(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Value1", result["Tag1"]);
            Assert.AreEqual("Value2", result["Tag2"]);
        }

        [TestMethod]
        public void UtilsGetPropertyDictListByPropertyNameList_ReturnsFilteredDictionary()
        {
            // Arrange
            var attributeCollection = new ObjectIdCollection();
            var mockAttId1 = new Mock<ObjectId>();
            var mockAttId2 = new Mock<ObjectId>();
            var mockAttId3 = new Mock<ObjectId>();
            
            var mockAttRef1 = new Mock<AttributeReference>();
            mockAttRef1.Setup(a => a.Tag).Returns("Tag1");
            mockAttRef1.Setup(a => a.TextString).Returns("Value1");
            
            var mockAttRef2 = new Mock<AttributeReference>();
            mockAttRef2.Setup(a => a.Tag).Returns("Tag2");
            mockAttRef2.Setup(a => a.TextString).Returns("Value2");
            
            var mockAttRef3 = new Mock<AttributeReference>();
            mockAttRef3.Setup(a => a.Tag).Returns("Tag3");
            mockAttRef3.Setup(a => a.TextString).Returns("Value3");
            
            mockAttId1.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef1.Object);
            mockAttId2.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef2.Object);
            mockAttId3.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef3.Object);
            
            attributeCollection.Add(mockAttId1.Object);
            attributeCollection.Add(mockAttId2.Object);
            attributeCollection.Add(mockAttId3.Object);
            
            _mockBlockRef.Setup(b => b.AttributeCollection).Returns(attributeCollection);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);
            
            var propertyNameList = new List<string> { "Tag1", "Tag3" };

            // Act
            var result = UtilsBlock.UtilsGetPropertyDictListByPropertyNameList(_mockObjectId.Object, propertyNameList);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Value1", result["Tag1"]);
            Assert.AreEqual("Value3", result["Tag3"]);
            Assert.IsFalse(result.ContainsKey("Tag2"));
        }

        [TestMethod]
        public void UtilsGetPropertyValueByPropertyName_ReturnsCorrectValue()
        {
            // Arrange
            var attributeCollection = new ObjectIdCollection();
            var mockAttId1 = new Mock<ObjectId>();
            var mockAttId2 = new Mock<ObjectId>();
            
            var mockAttRef1 = new Mock<AttributeReference>();
            mockAttRef1.Setup(a => a.Tag).Returns("Tag1");
            mockAttRef1.Setup(a => a.TextString).Returns("Value1");
            
            var mockAttRef2 = new Mock<AttributeReference>();
            mockAttRef2.Setup(a => a.Tag).Returns("Tag2");
            mockAttRef2.Setup(a => a.TextString).Returns("Value2");
            
            mockAttId1.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef1.Object);
            mockAttId2.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef2.Object);
            
            attributeCollection.Add(mockAttId1.Object);
            attributeCollection.Add(mockAttId2.Object);
            
            _mockBlockRef.Setup(b => b.AttributeCollection).Returns(attributeCollection);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetPropertyValueByPropertyName(_mockObjectId.Object, "Tag2");

            // Assert
            Assert.AreEqual("Value2", result);
        }

        [TestMethod]
        public void UtilsGetPropertyValueByPropertyName_WithNonExistentProperty_ReturnsEmptyString()
        {
            // Arrange
            var attributeCollection = new ObjectIdCollection();
            var mockAttId = new Mock<ObjectId>();
            
            var mockAttRef = new Mock<AttributeReference>();
            mockAttRef.Setup(a => a.Tag).Returns("Tag1");
            mockAttRef.Setup(a => a.TextString).Returns("Value1");
            
            mockAttId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(mockAttRef.Object);
            
            attributeCollection.Add(mockAttId.Object);
            
            _mockBlockRef.Setup(b => b.AttributeCollection).Returns(attributeCollection);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetPropertyValueByPropertyName(_mockObjectId.Object, "NonExistentTag");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void UtilsGetBlockRotaton_ReturnsCorrectRotation()
        {
            // Arrange
            double expectedRotation = 1.5;
            
            _mockBlockRef.Setup(b => b.Rotation).Returns(expectedRotation);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockRotaton(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(expectedRotation, result);
        }

        [TestMethod]
        public void UtilsGetBlockRotatonInDegrees_ReturnsCorrectRotationInDegrees()
        {
            // Arrange
            double rotationInRadians = Math.PI / 2; // 90 degrees
            double expectedRotationInDegrees = 90.0;
            
            _mockBlockRef.Setup(b => b.Rotation).Returns(rotationInRadians);
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForRead)).Returns(_mockBlockRef.Object);

            // Act
            var result = UtilsBlock.UtilsGetBlockRotatonInDegrees(_mockObjectId.Object);

            // Assert
            Assert.AreEqual(expectedRotationInDegrees, result, 0.001); // Using delta for floating point comparison
        }

        [TestMethod]
        public void UtilsSetBlockRotaton_SetsCorrectRotation()
        {
            // Arrange
            double expectedRotation = 2.0;
            
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForWrite)).Returns(_mockBlockRef.Object);

            // Act
            UtilsBlock.UtilsSetBlockRotaton(_mockObjectId.Object, expectedRotation);

            // Assert
            _mockBlockRef.VerifySet(b => b.Rotation = expectedRotation, Times.Once);
        }

        [TestMethod]
        public void UtilsSetBlockRotatonInDegrees_SetsCorrectRotationInRadians()
        {
            // Arrange
            double rotationInDegrees = 90.0;
            double expectedRotationInRadians = Math.PI / 2; // 90 degrees in radians
            
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForWrite)).Returns(_mockBlockRef.Object);

            // Act
            UtilsBlock.UtilsSetBlockRotatonInDegrees(_mockObjectId.Object, rotationInDegrees);

            // Assert
            _mockBlockRef.VerifySet(b => b.Rotation = It.Is<double>(r => Math.Abs(r - expectedRotationInRadians) < 0.001), Times.Once);
        }

        [TestMethod]
        public void UtilsSetBlockXYScale_SetsCorrectScale()
        {
            // Arrange
            double xScale = 2.0;
            double yScale = 3.0;
            Scale3d expectedScale = new Scale3d(xScale, yScale, 1.0);
            
            _mockObjectId.Setup(o => o.GetObject(OpenMode.ForWrite)).Returns(_mockBlockRef.Object);

            // Act
            UtilsBlock.UtilsSetBlockXYScale(_mockObjectId.Object, xScale, yScale);

            // Assert
            _mockBlockRef.VerifySet(b => b.ScaleFactors = It.Is<Scale3d>(s => 
                s.X == expectedScale.X && 
                s.Y == expectedScale.Y && 
                s.Z == expectedScale.Z), Times.Once);
        }
    }
} 