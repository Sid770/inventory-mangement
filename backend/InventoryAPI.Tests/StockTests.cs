using Xunit;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Controllers;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryAPI.Tests
{
    public class StockUpdateTests
    {
        private InventoryDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new InventoryDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task StockIn_IncreasesQuantityCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-001",
                Category = "Test",
                Price = 100m,
                Quantity = 10,
                MinimumStock = 5
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockIn",
                Quantity = 15,
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);

            // Assert
            var updatedItem = await context.InventoryItems.FindAsync(item.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(25, updatedItem.Quantity); // 10 + 15
        }

        [Fact]
        public async Task StockOut_DecreasesQuantityCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-002",
                Category = "Test",
                Price = 100m,
                Quantity = 50,
                MinimumStock = 10
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockOut",
                Quantity = 20,
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);

            // Assert
            var updatedItem = await context.InventoryItems.FindAsync(item.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(30, updatedItem.Quantity); // 50 - 20
        }

        [Fact]
        public async Task StockOut_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-003",
                Category = "Test",
                Price = 100m,
                Quantity = 5,
                MinimumStock = 2
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockOut",
                Quantity = 10, // More than available
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result.Result);
            var updatedItem = await context.InventoryItems.FindAsync(item.Id);
            Assert.Equal(5, updatedItem.Quantity); // Quantity unchanged
        }

        [Fact]
        public async Task Adjustment_SetsExactQuantity()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-004",
                Category = "Test",
                Price = 100m,
                Quantity = 100,
                MinimumStock = 10
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "Adjustment",
                Quantity = 75, // Set to exactly 75
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);

            // Assert
            var updatedItem = await context.InventoryItems.FindAsync(item.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(75, updatedItem.Quantity);
        }
    }

    public class AlertTriggeringTests
    {
        private InventoryDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new InventoryDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task LowStockAlert_TriggeredWhenQuantityAtMinimum()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-005",
                Category = "Test",
                Price = 100m,
                Quantity = 15,
                MinimumStock = 10
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockOut",
                Quantity = 5, // Will bring to exactly minimum
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);
            var okResult = result.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response.alert); // Alert should be triggered
        }

        [Fact]
        public async Task LowStockAlert_TriggeredWhenQuantityBelowMinimum()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-006",
                Category = "Test",
                Price = 100m,
                Quantity = 15,
                MinimumStock = 10
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockOut",
                Quantity = 8, // Will bring below minimum (to 7)
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);
            var okResult = result.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response.alert); // Alert should be triggered
        }

        [Fact]
        public async Task NoAlert_WhenQuantityAboveMinimum()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<StockController>>();
            var controller = new StockController(context, logger.Object);

            var item = new InventoryItem
            {
                Name = "Test Item",
                SKU = "TEST-007",
                Category = "Test",
                Price = 100m,
                Quantity = 50,
                MinimumStock = 10
            };
            context.InventoryItems.Add(item);
            await context.SaveChangesAsync();

            var transaction = new DTOs.StockTransactionDto
            {
                InventoryItemId = item.Id,
                Type = "StockOut",
                Quantity = 20, // Will keep quantity at 30 (above minimum)
                PerformedBy = "Test User"
            };

            // Act
            var result = await controller.ProcessStockTransaction(transaction);
            var okResult = result.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            var response = okResult.Value as dynamic;
            Assert.Null(response.alert); // No alert should be triggered
        }

        [Fact]
        public void IsLowStock_Property_WorksCorrectly()
        {
            // Arrange & Act
            var itemAboveMinimum = new InventoryItem
            {
                Quantity = 20,
                MinimumStock = 10
            };

            var itemAtMinimum = new InventoryItem
            {
                Quantity = 10,
                MinimumStock = 10
            };

            var itemBelowMinimum = new InventoryItem
            {
                Quantity = 5,
                MinimumStock = 10
            };

            // Assert
            Assert.False(itemAboveMinimum.IsLowStock);
            Assert.True(itemAtMinimum.IsLowStock);
            Assert.True(itemBelowMinimum.IsLowStock);
        }
    }
}
