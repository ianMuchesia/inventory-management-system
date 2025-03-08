using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs
{
    public class StockOperationDto
    {
        public int ProductId { get; set; }

        [Range(0.01, int.MaxValue, ErrorMessage = "Quantity in stock must be greater than zero")]

        public int Quantity { get; set; }
        public string Notes { get; set; }
    }
}