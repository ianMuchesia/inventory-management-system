






using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.ApplicationTests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryTransactionRepository> _mockInventoryTransactionRepository;

        private readonly Mock<IProductRepository> _mockProductRepository;

        private readonly Mock<ILogger<InventoryService>> _mockLogger;

        private readonly InventoryService _inventoryService;


        public InventoryServiceTests()
        {
            //set up mocks
            _mockInventoryTransactionRepository = new Mock<IInventoryTransactionRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<InventoryService>>();
            //create instance of InventoryService
            _inventoryService = new InventoryService(_mockInventoryTransactionRepository.Object, _mockProductRepository.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task AddStockAsync_WhenProductExists_ShouldIncreaseStockAndCreateTransaction()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Description", "Category", 10.00m, 10, 5);
            product.Id = productId;

            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5,
                Notes = "Adding stock"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockInventoryTransactionRepository.Setup(repo => repo.AddAsync(It.IsAny<InventoryTransaction>()))
                .Returns(Task.CompletedTask);
            _mockProductRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inventoryService.AddStockAsync(stockOperation);

            // Assert
            result.Should().BeTrue();
            product.QuantityInStock.Should().Be(15); // Initial 10 + 5 added

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.AddAsync(It.Is<InventoryTransaction>(t =>
                    t.ProductId == productId &&
                    t.Quantity == stockOperation.Quantity &&
                    t.Type == TransactionType.Addition &&
                    t.Notes == stockOperation.Notes)),
                Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task AddStockAsync_WhenProductDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            int productId = 1;
            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5,
                Notes = "Adding stock"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _inventoryService.AddStockAsync(stockOperation));

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.AddAsync(It.IsAny<InventoryTransaction>()), Times.Never);
            _mockProductRepository.Verify(repo =>
                repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task WithdrawStockAsync_WhenProductExistsAndHasEnoughStock_ShouldDecreaseStockAndCreateTransaction()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Description", "Category", 10.00m, 10, 5);
            product.Id = productId;

            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5,
                Notes = "Withdrawing stock"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockInventoryTransactionRepository.Setup(repo => repo.AddAsync(It.IsAny<InventoryTransaction>()))
                .Returns(Task.CompletedTask);
            _mockProductRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inventoryService.WithdrawStockAsync(stockOperation);

            // Assert
            result.Should().BeTrue();
            product.QuantityInStock.Should().Be(5); // Initial 10 - 5 withdrawn

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.AddAsync(It.Is<InventoryTransaction>(t =>
                    t.ProductId == productId &&
                    t.Quantity == stockOperation.Quantity &&
                    t.Type == TransactionType.Withdrawal &&
                    t.Notes == stockOperation.Notes)),
                Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task WithdrawStockAsync_WhenProductDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            int productId = 1;
            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5,
                Notes = "Withdrawing stock"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _inventoryService.WithdrawStockAsync(stockOperation));

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.AddAsync(It.IsAny<InventoryTransaction>()), Times.Never);
            _mockProductRepository.Verify(repo =>
                repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task WithdrawStockAsync_WhenInsufficientStock_ShouldThrowBadRequestException()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Description", "Category", 10.00m, 3, 5);
            product.Id = productId;

            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5, // Trying to withdraw more than available
                Notes = "Withdrawing stock"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _inventoryService.WithdrawStockAsync(stockOperation));

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.AddAsync(It.IsAny<InventoryTransaction>()), Times.Never);
            _mockProductRepository.Verify(repo =>
                repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task GetTransactionHistoryForProductAsync_WhenProductExists_ShouldReturnTransactions()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Description", "Category", 10.00m, 10, 5);
            product.Id = productId;

            var transactions = new List<InventoryTransaction>
            {
                new InventoryTransaction(productId, TransactionType.Addition, 5, "Initial stock"),
                new InventoryTransaction(productId, TransactionType.Withdrawal, 2, "Order fulfillment")
            };
            transactions[0].Id = 1;
            transactions[1].Id = 2;

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockInventoryTransactionRepository.Setup(repo => repo.GetByProductIdAsync(productId))
                .ReturnsAsync(transactions);

            // Act
            var result = await _inventoryService.GetTransactionHistoryForProductAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var transactionList = result.ToList();
            transactionList[0].ProductId.Should().Be(productId);
            transactionList[0].Type.Should().Be("Addition");
            transactionList[0].Quantity.Should().Be(5);
            transactionList[0].Notes.Should().Be("Initial stock");
            transactionList[0].ProductName.Should().Be(product.Name);

            transactionList[1].ProductId.Should().Be(productId);
            transactionList[1].Type.Should().Be("Withdrawal");
            transactionList[1].Quantity.Should().Be(2);
            transactionList[1].Notes.Should().Be("Order fulfillment");
            transactionList[1].ProductName.Should().Be(product.Name);

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo => repo.GetByProductIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetTransactionHistoryForProductAsync_WhenProductDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            int productId = 1;

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _inventoryService.GetTransactionHistoryForProductAsync(productId));

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockInventoryTransactionRepository.Verify(repo =>
                repo.GetByProductIdAsync(productId), Times.Never);
        }

    }
}