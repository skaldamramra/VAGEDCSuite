using System;
using System.Drawing;
using NUnit.Framework;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Tests
{
    [TestFixture]
    public class MapViewerExIntegrationTests
    {
        [Test]
        public void CreateMapViewerState_FromEmptyData_Succeeds()
        {
            // Arrange
            var state = new MapViewerState();
            
            // Assert
            Assert.IsNotNull(state);
            Assert.IsNull(state.Data);
            Assert.IsNull(state.Metadata);
            Assert.IsNull(state.Axes);
            Assert.IsNull(state.Configuration);
        }
        
        [Test]
        public void CreateMapData_WithContent_Succeeds()
        {
            // Arrange
            var data = new MapData
            {
                Content = new byte[] { 0x10, 0x20, 0x30, 0x40 },
                TableWidth = 2,
                IsSixteenBit = false,
                Length = 4
            };
            
            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual(4, data.Content.Length);
        }
        
        [Test]
        public void CreateMapMetadata_WithProperties_Succeeds()
        {
            // Arrange
            var metadata = new MapMetadata
            {
                Name = "TestMap",
                Description = "A test map",
                Category = VAGSuite.XDFCategories.Fuel,
                XAxisName = "RPM",
                YAxisName = "Load",
                ZAxisName = "Fuel"
            };
            
            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual("TestMap", metadata.Name);
            Assert.AreEqual(VAGSuite.XDFCategories.Fuel, metadata.Category);
        }
        
        [Test]
        public void CreateViewConfiguration_WithDefaults_Succeeds()
        {
            // Arrange
            var config = new ViewConfiguration();
            
            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(ViewType.Hexadecimal, config.ViewType);
            Assert.AreEqual(ViewSize.NormalView, config.ViewSize);
            Assert.IsFalse(config.IsUpsideDown);
            Assert.IsFalse(config.DisableColors);
        }
        
        [Test]
        public void CreateAxisData_WithValues_Succeeds()
        {
            // Arrange
            var axes = new AxisData
            {
                XAxisValues = new int[] { 1000, 2000, 3000, 4000 },
                YAxisValues = new int[] { 10, 20, 30, 40 }
            };
            
            // Assert
            Assert.IsNotNull(axes);
            Assert.AreEqual(4, axes.XAxisValues.Length);
            Assert.AreEqual(4, axes.YAxisValues.Length);
        }
        
        [Test]
        public void MapData_Clone_Succeeds()
        {
            // Arrange
            var data = new MapData
            {
                Content = new byte[] { 0x10, 0x20, 0x30, 0x40 },
                OriginalContent = new byte[] { 0x11, 0x21, 0x31, 0x41 },
                CompareContent = new byte[] { 0x12, 0x22, 0x32, 0x42 },
                Address = 0x1000,
                SramAddress = 0x2000,
                Length = 4,
                IsSixteenBit = false,
                TableWidth = 2
            };
            
            // Act
            var clone = data.Clone();
            
            // Assert
            Assert.IsNotNull(clone);
            Assert.AreEqual(4, clone.Content.Length);
            Assert.AreEqual(4, clone.OriginalContent.Length);
            Assert.AreEqual(4, clone.CompareContent.Length);
            Assert.AreEqual(0x1000, clone.Address);
        }
        
        [Test]
        public void MapData_CloneWithNullContent_Succeeds()
        {
            // Arrange
            var data = new MapData
            {
                Content = null,
                OriginalContent = null,
                CompareContent = null
            };
            
            // Act
            var clone = data.Clone();
            
            // Assert
            Assert.IsNotNull(clone);
            Assert.IsNull(clone.Content);
        }
        
        [Test]
        public void DataConversionService_RoundTrip8Bit_Succeeds()
        {
            // Arrange
            var conversionService = new DataConversionService();
            var data = new MapData
            {
                Content = new byte[] { 0x10, 0x20, 0x30, 0x40 },
                TableWidth = 2,
                IsSixteenBit = false,
                Length = 4
            };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var dataTable = conversionService.ConvertToDataTable(data, config);
            var result = conversionService.ConvertFromDataTable(dataTable, data, config);
            
            // Assert
            Assert.AreEqual(4, result.Length);
        }
        
        [Test]
        public void DataConversionService_RoundTrip16Bit_Succeeds()
        {
            // Arrange
            var conversionService = new DataConversionService();
            var data = new MapData
            {
                Content = new byte[] { 0x00, 0x10, 0x00, 0x20 },
                TableWidth = 2,
                IsSixteenBit = true,
                Length = 4
            };
            var config = new ViewConfiguration { ViewType = ViewType.Hexadecimal };
            
            // Act
            var dataTable = conversionService.ConvertToDataTable(data, config);
            var result = conversionService.ConvertFromDataTable(dataTable, data, config);
            
            // Assert
            Assert.AreEqual(4, result.Length);
        }
        
        [Test]
        public void MapRenderingService_CalculateCellColor_StandardMode_Succeeds()
        {
            // Arrange
            var renderingService = new MapRenderingService();
            
            // Act
            Color color = renderingService.CalculateCellColor(128, 255, false, false);
            
            // Assert
            Assert.AreNotEqual(Color.White, color);
        }
        
        [Test]
        public void Services_CanBeCreated_WithDefaultConstructor()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => new DataConversionService());
            Assert.DoesNotThrow(() => new MapRenderingService());
            Assert.DoesNotThrow(() => new ChartService());
            Assert.DoesNotThrow(() => new ClipboardService(new DataConversionService()));
            Assert.DoesNotThrow(() => new SmoothingService(new DataConversionService()));
        }
    }
}