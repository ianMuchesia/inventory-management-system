



using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Persistence.Interfaces
{
    public interface IInventoryTransactionRepository
    {
        Task<InventoryTransaction?> GetByIdAsync(int id);
        Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId);
        Task AddAsync(InventoryTransaction transaction);

        Task SaveChangesAsync();

        IQueryable<InventoryTransaction> AllTransactionsQuery();
    }
}