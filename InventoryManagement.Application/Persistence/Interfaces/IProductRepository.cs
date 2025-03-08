


using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Persistence.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);

        Task<Product?> GetByNameAsync(string name);

        IQueryable<Product> AllProductsQeury();

        Task SaveChangesAsync();
    }
}