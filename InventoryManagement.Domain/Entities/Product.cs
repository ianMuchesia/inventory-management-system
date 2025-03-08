// Purpose: Contains the Product entity class which represents a product in the inventory management system.

using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }

        public DateTime CreatedAt { get; private set; }


        public DateTime? UpdatedAt { get; private set; }

        public ICollection<InventoryTransaction> InventoryTransactions { get; private set; } = new List<InventoryTransaction>();



        public Product(string name, string description, string category, decimal unitPrice, int quantityInStock, int reorderLevel)
        {
            Name = name;
            Description = description;
            Category = category;
            UnitPrice = unitPrice;
            QuantityInStock = quantityInStock;
            ReorderLevel = reorderLevel;
            CreatedAt = DateTime.UtcNow;

        }

        // Domain logic for updating stock
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            QuantityInStock += quantity;
        }

        public bool WithdrawStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            if (QuantityInStock < quantity)
                return false;

            QuantityInStock -= quantity;
            return true;
        }

        public bool IsLowStock()
        {
            return QuantityInStock <= ReorderLevel;
        }

        // Update product details
        public void Update(string name, string description, string category, decimal unitPrice, int reorderLevel)
        {
            Name = name;
            Description = description;
            Category = category;
            UnitPrice = unitPrice;
            ReorderLevel = reorderLevel;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
