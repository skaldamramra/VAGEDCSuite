using System;
using System.Data;
using NUnit.Framework;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Tests
{
    [TestFixture]
    public class DataConversionServiceTests
    {
        private IDataConversionService _service;
        
        [SetUp]
        public void Setup()
        {
            _service = new DataConversionService();
        }
        
        [Test]
        public void ConvertToDataTable_8BitData_CreatesCorrectTable()
        {
            // Arrange
            var data = new MapData
            {
                Content = new byte[] { 0x10, 0x20, 0x30, 0x40 },
                TableWidth = 2,
                IsSixteenBit = false,
                Length = 4
            };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var result = _service.ConvertToDataTable(data, config);
            
            // Assert
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(2, result.Columns.Count);
            Assert.AreEqual("10", result.Rows[0][0]);
            Assert.AreEqual("20", result.Rows[0][1]);
        }
        
        [Test]
        public void ConvertToDataTable_16BitData_CreatesCorrectTable()
        {
            // Arrange
            var data = new MapData
            {
                Content = new byte[] { 0x00, 0x10, 0x00, 0x20 },
                TableWidth = 2,
                IsSixteenBit = true,
                Length = 4
            };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var result = _service.ConvertToDataTable(data, config);
            
            // Assert
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(2, result.Columns.Count);
            Assert.AreEqual("0010", result.Rows[0][0]);
            Assert.AreEqual("0020", result.Rows[0][1]);
        }
        
        [Test]
        public void FormatValue_HexView_FormatsCorrectly()
        {
            // Arrange
            int value = 255;
            
            // Act
            var result = _service.FormatValue(value, ViewType.Hexadecimal, false);
            
            // Assert
            Assert.AreEqual("FF", result);
        }
        
        [Test]
        public void FormatValue_HexView16Bit_FormatsCorrectly()
        {
            // Arrange
            int value = 4096;
            
            // Act
            var result = _service.FormatValue(value, ViewType.Hexadecimal, true);
            
            // Assert
            Assert.AreEqual("1000", result);
        }
        
        [Test]
        public void FormatValue_DecimalView_FormatsCorrectly()
        {
            // Arrange
            int value = 123;
            
            // Act
            var result = _service.FormatValue(value, ViewType.Decimal, false);
            
            // Assert
            Assert.AreEqual("123", result);
        }
        
        [Test]
        public void FormatValue_EasyView_ConvertsCorrectly()
        {
            // Arrange
            int value = 1500;
            
            // Act
            var result = _service.FormatValue(value, ViewType.Easy, false);
            
            // Assert
            Assert.AreEqual("15", result); // 1500 / 100 = 15
        }
        
        [Test]
        public void FormatValue_ASCIIView_ConvertsCorrectly()
        {
            // Arrange
            int value = 65; // 'A'
            
            // Act
            var result = _service.FormatValue(value, ViewType.ASCII, false);
            
            // Assert
            Assert.AreEqual("A", result);
        }
        
        [Test]
        public void ParseValue_HexView_ParsesCorrectly()
        {
            // Arrange
            string value = "FF";
            
            // Act
            var result = _service.ParseValue(value, ViewType.Hexadecimal);
            
            // Assert
            Assert.AreEqual(255, result);
        }
        
        [Test]
        public void ParseValue_DecimalView_ParsesCorrectly()
        {
            // Arrange
            string value = "123";
            
            // Act
            var result = _service.ParseValue(value, ViewType.Decimal);
            
            // Assert
            Assert.AreEqual(123, result);
        }
        
        [Test]
        public void ParseValue_EasyView_ParsesCorrectly()
        {
            // Arrange
            string value = "15";
            
            // Act
            var result = _service.ParseValue(value, ViewType.Easy);
            
            // Assert
            Assert.AreEqual(1500, result); // 15 * 100 = 1500
        }
        
        [Test]
        public void ParseValue_ASCIIView_ParsesCorrectly()
        {
            // Arrange
            string value = "A";
            
            // Act
            var result = _service.ParseValue(value, ViewType.ASCII);
            
            // Assert
            Assert.AreEqual(65, result); // 'A' = 65
        }
        
        [Test]
        public void ApplyCorrection_NoCorrection_ReturnsOriginal()
        {
            // Arrange
            int value = 100;
            
            // Act
            var result = _service.ApplyCorrection(value, 1.0, 0.0);
            
            // Assert
            Assert.AreEqual(100, result);
        }
        
        [Test]
        public void ApplyCorrection_WithFactor_AppliesCorrectly()
        {
            // Arrange
            int value = 100;
            
            // Act
            var result = _service.ApplyCorrection(value, 1.5, 0.0);
            
            // Assert
            Assert.AreEqual(150, result);
        }
        
        [Test]
        public void ApplyCorrection_WithOffset_AppliesCorrectly()
        {
            // Arrange
            int value = 100;
            
            // Act
            var result = _service.ApplyCorrection(value, 1.0, 10.0);
            
            // Assert
            Assert.AreEqual(110, result);
        }
        
        [Test]
        public void ConvertToDataTable_NullData_ReturnsEmptyTable()
        {
            // Arrange
            var data = new MapData { Content = null };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var result = _service.ConvertToDataTable(data, config);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Rows.Count);
        }
        
        [Test]
        public void ConvertFromDataTable_RoundTrip8Bit_Succeeds()
        {
            // Arrange
            var data = new MapData
            {
                Content = new byte[] { 0x10, 0x20, 0x30, 0x40 },
                TableWidth = 2,
                IsSixteenBit = false,
                Length = 4
            };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var dataTable = _service.ConvertToDataTable(data, config);
            var result = _service.ConvertFromDataTable(dataTable, data, config);
            
            // Assert
            Assert.AreEqual(4, result.Length);
        }
    }
}