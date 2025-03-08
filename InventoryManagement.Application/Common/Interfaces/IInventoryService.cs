

using InventoryManagement.Application.DTOs;

namespace InventoryManagement.Application.Common.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> AddStockAsync(StockOperationDto stockOperation);
        Task<bool> WithdrawStockAsync(StockOperationDto stockOperation);
        Task<IEnumerable<InventoryTransactionDto>> GetTransactionHistoryForProductAsync(int productId);
    }
}