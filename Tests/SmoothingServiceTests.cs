using System;
using NUnit.Framework;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Tests
{
    [TestFixture]
    public class SmoothingServiceTests
    {
        private ISmoothingService _service;
        private IDataConversionService _conversionService;
        
        [SetUp]
        public void Setup()
        {
            _conversionService = new DataConversionService();
            _service = new SmoothingService(_conversionService);
        }
        
        [Test]
        public void SmoothLinear_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.SmoothLinear(null, null);
        }
        
        [Test]
        public void SmoothLinear_SingleCell_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[] { 100 };
            
            // Act & Assert - should not throw with single cell
            _service.SmoothLinear(cells, null);
        }
        
        [Test]
        public void SmoothLinear_TwoCells_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[] { 100, 200 };
            
            // Act & Assert - should not throw with two cells (no interpolation needed)
            _service.SmoothLinear(cells, null);
        }
        
        [Test]
        public void SmoothProportional_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.SmoothProportional(null, null, null, null);
        }
        
        [Test]
        public void SmoothProportional_EmptyCells_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[0];
            
            // Act & Assert - should not throw with empty cells
            _service.SmoothProportional(cells, null, null, null);
        }
        
        [Test]
        public void SmoothProportional_NullAxis_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[] { 100, 200 };
            
            // Act & Assert - should not throw with null axis arrays
            _service.SmoothProportional(cells, null, null, null);
        }
        
        [Test]
        public void SmoothLinearWithNeighbors_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.SmoothLinearWithNeighbors(null, null, null, null);
        }
        
        [Test]
        public void SmoothLinearWithNeighbors_SingleCell_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[] { 100 };
            
            // Act & Assert - should not throw with single cell
            _service.SmoothLinearWithNeighbors(cells, null, null, null);
        }
        
        [Test]
        public void SmoothProportionalWithNeighbors_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.SmoothProportionalWithNeighbors(null, null, null, null);
        }
        
        [Test]
        public void SmoothProportionalWithNeighbors_EmptyCells_DoesNotThrow()
        {
            // Arrange
            object[] cells = new object[0];
            
            // Act & Assert - should not throw with empty cells
            _service.SmoothProportionalWithNeighbors(cells, null, null, null);
        }
        
        [Test]
        public void SmoothBilinear_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.SmoothBilinear(null, null, null, null);
        }
        
        [Test]
        public void SmoothBilinear_LessThanFourCells_DoesNotThrow()
        {
            // Arrange - need at least 4 cells for bilinear
            object[] cells = new object[] { 100, 200, 300 };
            
            // Act & Assert - should not throw with less than 4 cells
            _service.SmoothBilinear(cells, null, null, null);
        }
    }
}