using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Entities
{
    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Notes { get; set; }
        
        public Product? Product { get; set; }
        
        // Default constructor needed for EF Core
        public InventoryTransaction() { }
        
        public InventoryTransaction(int productId, TransactionType type, int quantity, string notes = "")
        {
            ProductId = productId;
            Type = type;
            Quantity = quantity;
            TransactionDate = DateTime.UtcNow;
            Notes = notes ?? string.Empty;
        }
    }
}