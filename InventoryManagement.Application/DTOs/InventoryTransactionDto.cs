using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.DTOs
{
     public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Notes { get; set; }
    }
}