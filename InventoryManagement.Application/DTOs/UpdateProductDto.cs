using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs
{
     public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than zero")]

        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]

        public int ReorderLevel { get; set; }
    }
}