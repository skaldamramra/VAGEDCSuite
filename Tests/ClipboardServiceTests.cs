using System;
using System.Collections.Generic;
using NUnit.Framework;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Tests
{
    [TestFixture]
    public class ClipboardServiceTests
    {
        private IClipboardService _service;
        private IDataConversionService _conversionService;
        
        [SetUp]
        public void Setup()
        {
            _conversionService = new DataConversionService();
            _service = new ClipboardService(_conversionService);
        }
        
        [Test]
        public void CopySelection_NullCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null cells
            _service.CopySelection(null, ViewType.Hexadecimal);
        }
        
        [Test]
        public void CopySelection_EmptyCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with empty cells
            _service.CopySelection(new object[0], ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtCurrentLocation_NullTargetCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null target cells
            _service.PasteAtCurrentLocation(null, ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtOriginalPosition_NullTargetCells_DoesNotThrow()
        {
            // Act & Assert - should not throw with null target cells
            _service.PasteAtOriginalPosition(null, ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtCurrentLocation_EmptyTargetCells_HandlesGracefully()
        {
            // Arrange
            object[] targetCells = new object[0];
            
            // Act & Assert - should not throw with empty target cells
            _service.PasteAtCurrentLocation(targetCells, ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtOriginalPosition_EmptyTargetCells_HandlesGracefully()
        {
            // Arrange
            object[] targetCells = new object[0];
            
            // Act & Assert - should not throw with empty target cells
            _service.PasteAtOriginalPosition(targetCells, ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtCurrentLocation_SingleElementTarget_HandlesGracefully()
        {
            // Arrange
            object[] targetCells = new object[1];
            
            // Act & Assert - should not throw with single element target
            _service.PasteAtCurrentLocation(targetCells, ViewType.Hexadecimal);
        }
        
        [Test]
        public void PasteAtOriginalPosition_SingleElementTarget_HandlesGracefully()
        {
            // Arrange
            object[] targetCells = new object[1];
            
            // Act & Assert - should not throw with single element target
            _service.PasteAtOriginalPosition(targetCells, ViewType.Hexadecimal);
        }
    }
}