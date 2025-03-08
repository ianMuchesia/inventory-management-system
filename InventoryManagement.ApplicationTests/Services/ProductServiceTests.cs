// Purpose: Contains tests for ProductService.cs.
using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.ApplicationTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;

        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            //set up mocks
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            //create instance of ProductService
            _productService = new ProductService(_mockProductRepository.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task CreateProductAsync_ShouldReturnNewProduct()
        {
            // Arrange
            var productDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Category = "Test Category",
                UnitPrice = 10.00m,
                QuantityInStock = 100,
                ReorderLevel = 10
            };

            _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateProductAsync(productDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(productDto.Name);
            result.Description.Should().Be(productDto.Description);
            result.Category.Should().Be(productDto.Category);
            result.UnitPrice.Should().Be(productDto.UnitPrice);
            result.QuantityInStock.Should().Be(productDto.QuantityInStock);
            result.ReorderLevel.Should().Be(productDto.ReorderLevel);

            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product("Product 1", "Description 1", "Category 1", 10.00m, 100, 10),
                new Product("Product 2", "Description 2", "Category 2", 20.00m, 200, 20)
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var productList = result.ToList();
            productList[0].Name.Should().Be("Product 1");
            productList[1].Name.Should().Be("Product 2");

            _mockProductRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }


        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Test Description", "Test Category", 10.00m, 100, 10);
            product.Id = productId;

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be(product.Name);

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            int productId = 1;

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _productService.GetProductByIdAsync(productId));

            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }


        [Fact]
        public async Task UpdateProductAsync_WhenProductExists_ShouldReturnUpdatedProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Original Name", "Original Description", "Original Category", 10.00m, 100, 10);
            product.Id = productId;

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Category = "Updated Category",
                UnitPrice = 20.00m,
                ReorderLevel = 20
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockProductRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be(updateDto.Name);
            result.Description.Should().Be(updateDto.Description);
            result.Category.Should().Be(updateDto.Category);
            result.UnitPrice.Should().Be(updateDto.UnitPrice);
            result.ReorderLevel.Should().Be(updateDto.ReorderLevel);
            
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }


        [Fact]
        public async Task DeleteProductAsync_WhenProductExists_ShouldDeleteProduct()
        {
            // Arrange
            int productId = 1;
            var product = new Product("Test Product", "Test Description", "Test Category", 10.00m, 100, 10);
            product.Id = productId;

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockProductRepository.Setup(repo => repo.DeleteAsync(product))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.DeleteProductAsync(productId);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(product), Times.Once);
        }

        [Fact]
        public async Task GetInventoryValuationAsync_ShouldReturnCorrectValuation()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product("Product 1", "Description 1", "Category 1", 10.00m, 10, 5),
                new Product("Product 2", "Description 2", "Category 2", 20.00m, 20, 10)
            };
            products[0].Id = 1;
            products[1].Id = 2;

            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetInventoryValuationAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalProducts.Should().Be(2);
            result.TotalItems.Should().Be(30); // 10 + 20
            result.TotalValue.Should().Be(500.00m); // (10*10) + (20*20)
            result.Products.Should().HaveCount(2);
            
            _mockProductRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnLowStockProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product("Low Stock Product", "Description", "Category", 10.00m, 5, 10),
                new Product("Normal Stock Product", "Description", "Category", 20.00m, 20, 10)
            };

            _mockProductRepository.Setup(repo => repo.GetLowStockProductsAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetLowStockProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            _mockProductRepository.Verify(repo => repo.GetLowStockProductsAsync(), Times.Once);
        }

    }
}