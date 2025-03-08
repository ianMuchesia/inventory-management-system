




using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Common.Interfaces
{
    public interface IProductService
    {

        Task<PagedResult<ProductDto>> SearchProductsAsync(SearchDto searchDto);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto productDto);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        Task<InventoryValuationDto> GetInventoryValuationAsync();
    }
}