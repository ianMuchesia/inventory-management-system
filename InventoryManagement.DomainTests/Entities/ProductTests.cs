


using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.DomainTests.Entities
{

    public class ProductTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            string name = "Test Product";
            string description = "Test Description";
            string category = "Test Category";
            decimal unitPrice = 10.99m;
            int quantityInStock = 100;
            int reorderLevel = 20;

            // Act
            var product = new Product(name, description, category, unitPrice, quantityInStock, reorderLevel);

            // Assert
            Assert.Equal(name, product.Name);
            Assert.Equal(description, product.Description);
            Assert.Equal(category, product.Category);
            Assert.Equal(unitPrice, product.UnitPrice);
            Assert.Equal(quantityInStock, product.QuantityInStock);
            Assert.Equal(reorderLevel, product.ReorderLevel);
            Assert.True(DateTime.UtcNow.Subtract(product.CreatedAt).TotalSeconds < 1);
            Assert.Null(product.UpdatedAt);
            Assert.Empty(product.InventoryTransactions);
        }

        [Fact]
        public void AddStock_WithPositiveQuantity_ShouldIncreaseStock()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 100, 20);
            int addQuantity = 50;
            int expectedQuantity = 150;

            // Act
            product.AddStock(addQuantity);

            // Assert
            Assert.Equal(expectedQuantity, product.QuantityInStock);
        }

        [Fact]
        public void AddStock_WithZeroOrNegativeQuantity_ShouldThrowDomainException()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 100, 20);

            // Act & Assert
            Assert.Throws<DomainException>(() => product.AddStock(0));
            Assert.Throws<DomainException>(() => product.AddStock(-10));
        }

        [Fact]
        public void WithdrawStock_WithValidQuantity_ShouldDecreaseStockAndReturnTrue()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 100, 20);
            int withdrawQuantity = 50;
            int expectedQuantity = 50;

            // Act
            var result = product.WithdrawStock(withdrawQuantity);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedQuantity, product.QuantityInStock);
        }

        [Fact]
        public void WithdrawStock_WithInsufficientQuantity_ShouldNotChangeStockAndReturnFalse()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 100, 20);
            int withdrawQuantity = 150;
            int expectedQuantity = 100;

            // Act
            var result = product.WithdrawStock(withdrawQuantity);

            // Assert
            Assert.False(result);
            Assert.Equal(expectedQuantity, product.QuantityInStock);
        }

        [Fact]
        public void WithdrawStock_WithZeroOrNegativeQuantity_ShouldThrowDomainException()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 100, 20);

            // Act & Assert
            Assert.Throws<DomainException>(() => product.WithdrawStock(0));
            Assert.Throws<DomainException>(() => product.WithdrawStock(-10));
        }

        [Fact]
        public void IsLowStock_WhenQuantityBelowReorderLevel_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 15, 20);

            // Act
            var result = product.IsLowStock();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLowStock_WhenQuantityEqualsReorderLevel_ShouldReturnTrue()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 20, 20);

            // Act
            var result = product.IsLowStock();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsLowStock_WhenQuantityAboveReorderLevel_ShouldReturnFalse()
        {
            // Arrange
            var product = new Product("Test", "Description", "Category", 10.0m, 30, 20);

            // Act
            var result = product.IsLowStock();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Update_ShouldUpdatePropertiesAndSetUpdatedAt()
        {
            // Arrange
            var product = new Product("Old Name", "Old Desc", "Old Cat", 10.0m, 100, 20);
            string newName = "New Name";
            string newDescription = "New Description";
            string newCategory = "New Category";
            decimal newUnitPrice = 15.99m;
            int newReorderLevel = 25;

            // Act
            product.Update(newName, newDescription, newCategory, newUnitPrice, newReorderLevel);

            // Assert
            Assert.Equal(newName, product.Name);
            Assert.Equal(newDescription, product.Description);
            Assert.Equal(newCategory, product.Category);
            Assert.Equal(newUnitPrice, product.UnitPrice);
            Assert.Equal(newReorderLevel, product.ReorderLevel);
            Assert.NotNull(product.UpdatedAt);
            Assert.True(DateTime.UtcNow.Subtract(product.UpdatedAt.Value).TotalSeconds < 1);
            // Quantity should remain unchanged
            Assert.Equal(100, product.QuantityInStock);
        }
    }
}