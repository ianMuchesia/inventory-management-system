namespace InventoryManagement.Application.DTOs
{
     public class ProductValuationDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int QuantityInStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }
}