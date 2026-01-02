using System;
using System.Drawing;
using NUnit.Framework;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Tests
{
    [TestFixture]
    public class MapRenderingServiceTests
    {
        private IMapRenderingService _service;
        
        [SetUp]
        public void Setup()
        {
            _service = new MapRenderingService();
        }
        
        [Test]
        public void CalculateCellColor_ZeroMaxValue_ReturnsWhite()
        {
            // Arrange
            int value = 100;
            int maxValue = 0;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, false);
            
            // Assert
            Assert.AreEqual(Color.White, result);
        }
        
        [Test]
        public void CalculateCellColor_StandardMode_ReturnsCorrectColor()
        {
            // Arrange
            int value = 128;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, false);
            
            // Assert
            // At 128/255, red=128, green=127, blue=0
            Assert.AreEqual(128, result.R);
            Assert.AreEqual(127, result.G);
            Assert.AreEqual(0, result.B);
        }
        
        [Test]
        public void CalculateCellColor_OnlineMode_ReturnsCorrectColor()
        {
            // Arrange
            int value = 64;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, true, false);
            
            // Assert
            // At 64/255 normalized = 64, red=32, green=191, blue=191
            Assert.AreEqual(32, result.R);
            Assert.AreEqual(191, result.G);
            Assert.AreEqual(191, result.B);
        }
        
        [Test]
        public void CalculateCellColor_RedWhiteMode_ReturnsCorrectColor()
        {
            // Arrange
            int value = 128;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, true);
            
            // Assert
            // At 128/255, red=128, green=127, blue=127
            Assert.AreEqual(128, result.R);
            Assert.AreEqual(127, result.G);
            Assert.AreEqual(127, result.B);
        }
        
        [Test]
        public void CalculateCellColor_ValueAtMax_ReturnsRed()
        {
            // Arrange
            int value = 255;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, false);
            
            // Assert
            Assert.AreEqual(255, result.R);
            Assert.AreEqual(0, result.G);
            Assert.AreEqual(0, result.B);
        }
        
        [Test]
        public void CalculateCellColor_ValueAtZero_ReturnsGreen()
        {
            // Arrange
            int value = 0;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, false);
            
            // Assert
            Assert.AreEqual(0, result.R);
            Assert.AreEqual(255, result.G);
            Assert.AreEqual(0, result.B);
        }
        
        [Test]
        public void CalculateCellColor_ValueExceedsMax_ReturnsRed()
        {
            // Arrange
            int value = 300;
            int maxValue = 255;
            
            // Act
            var result = _service.CalculateCellColor(value, maxValue, false, false);
            
            // Assert
            Assert.AreEqual(255, result.R);
            Assert.AreEqual(0, result.G);
            Assert.AreEqual(0, result.B);
        }
        
        [Test]
        public void FormatCellDisplayText_SecondsFormat_ConvertsCorrectly()
        {
            // Arrange
            var config = new ViewConfiguration();
            var metadata = new MapMetadata { ZAxisName = "Seconds" };
            
            // Act
            var result = _service.FormatCellDisplayText(1500, config, metadata, true);
            
            // Assert
            Assert.AreEqual("15", result); // 1500 / 100 = 15
        }
        
        [Test]
        public void FormatCellDisplayText_DegreesFormat_ConvertsCorrectly()
        {
            // Arrange
            var config = new ViewConfiguration();
            var metadata = new MapMetadata { ZAxisName = "Degrees" };
            
            // Act
            var result = _service.FormatCellDisplayText(128, config, metadata, false);
            
            // Assert
            Assert.AreEqual("0", result); // 128 - 128 = 0
        }
        
        [Test]
        public void FormatCellDisplayText_NullMetadata_ReturnsValue()
        {
            // Arrange
            var config = new ViewConfiguration();
            
            // Act
            var result = _service.FormatCellDisplayText(100, config, null, false);
            
            // Assert
            Assert.AreEqual("100", result);
        }
        
        [Test]
        public void ShouldShowOpenLoopIndicator_NullOpenLoop_ReturnsFalse()
        {
            // Arrange
            int[] xAxis = { 1, 2, 3 };
            int[] yAxis = { 10, 20, 30 };
            
            // Act
            var result = _service.ShouldShowOpenLoopIndicator(0, 0, null, xAxis, yAxis, "mg/c", "rpm");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void ShouldShowOpenLoopIndicator_EmptyArrays_ReturnsFalse()
        {
            // Arrange
            byte[] openLoop = { 1, 2, 3, 4 };
            
            // Act
            var result = _service.ShouldShowOpenLoopIndicator(0, 0, openLoop, null, null, "mg/c", "rpm");
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [Test]
        public void ShouldShowOpenLoopIndicator_MatchFound_ReturnsTrue()
        {
            // Arrange
            byte[] openLoop = { 1, 2, 3, 4 }; // x=1, y=2 and x=3, y=4 pairs
            int[] xAxis = { 1, 2, 3 };
            int[] yAxis = { 10, 20, 30 };
            
            // Act - row 0 (yAxis[2]=30), col 0 (xAxis[2]=3) should match openLoop[2]=3, openLoop[3]=4
            var result = _service.ShouldShowOpenLoopIndicator(1, 0, openLoop, xAxis, yAxis, "mg/c", "rpm");
            
            // Assert - row 1 means yAxis[1]=20, col 0 means xAxis[0]=1
            // This should match openLoop[0]=1, openLoop[1]=2
            Assert.IsTrue(result);
        }
        
        [Test]
        public void ShouldShowOpenLoopIndicator_NoMatch_ReturnsFalse()
        {
            // Arrange
            byte[] openLoop = { 10, 20 }; // x=10, y=20 pair
            int[] xAxis = { 1, 2, 3 };
            int[] yAxis = { 10, 20, 30 };
            
            // Act - row 0 (yAxis[2]=30), col 0 (xAxis[2]=3) should NOT match
            var result = _service.ShouldShowOpenLoopIndicator(1, 0, openLoop, xAxis, yAxis, "mg/c", "rpm");
            
            // Assert
            Assert.IsFalse(result);
        }
    }
}