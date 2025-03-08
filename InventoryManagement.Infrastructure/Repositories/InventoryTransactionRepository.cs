
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {


        private readonly  InventoryDbContext _context;

        public InventoryTransactionRepository(InventoryDbContext context)
        {
            _context = context;
        }
        public  async Task AddAsync(InventoryTransaction transaction)
        {
            await _context.InventoryTransactions.AddAsync(transaction);
        }

        public IQueryable<InventoryTransaction> AllTransactionsQuery()
        {
            return _context.InventoryTransactions;
        }

        public async Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            
            return await _context.InventoryTransactions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(int productId)
        {
            return await _context.InventoryTransactions.Where(x => x.ProductId == productId).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}