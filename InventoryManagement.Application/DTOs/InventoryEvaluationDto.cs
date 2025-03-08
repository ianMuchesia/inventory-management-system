namespace InventoryManagement.Application.DTOs
{
    public class InventoryValuationDto
    {
        public int TotalProducts { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public List<ProductValuationDto> Products { get; set; }
    }
}