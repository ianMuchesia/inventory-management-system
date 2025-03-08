




using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.DomainTests.Entities
{
    public class InventoryTransactionTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            int productId = 1;
            TransactionType type = TransactionType.Addition;
            int quantity = 10;
            string notes = "Test notes";

            // Act
            var transaction = new InventoryTransaction(productId, type, quantity, notes);

            // Assert
            Assert.Equal(productId, transaction.ProductId);
            Assert.Equal(type, transaction.Type);
            Assert.Equal(quantity, transaction.Quantity);
            Assert.Equal(notes, transaction.Notes);
            Assert.True(DateTime.UtcNow.Subtract(transaction.TransactionDate).TotalSeconds < 1);
        }

        [Fact]
        public void Constructor_WithoutNotes_ShouldSetDefaultEmptyNotes()
        {
            // Arrange
            int productId = 1;
            TransactionType type = TransactionType.Addition;
            int quantity = 5;

            // Act
            var transaction = new InventoryTransaction(productId, type, quantity);

            // Assert
            Assert.Equal(string.Empty, transaction.Notes);
        }

        [Fact]
        public void Constructor_ShouldSetTransactionDateToCurrentUtcTime()
        {
            // Arrange
            int productId = 1;
            TransactionType type = TransactionType.Withdrawal;
            int quantity = 10;

            // Act
            var before = DateTime.UtcNow;
            var transaction = new InventoryTransaction(productId, type, quantity);
            var after = DateTime.UtcNow;

            // Assert
            Assert.True(transaction.TransactionDate >= before);
            Assert.True(transaction.TransactionDate <= after);
        }
    }
}