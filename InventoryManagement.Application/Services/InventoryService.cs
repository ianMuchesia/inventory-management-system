// Purpose: Contains the implementation of the IInventoryService interface. This class is responsible for handling the business logic for the inventory management system. It is responsible for adding and withdrawing stock from the inventory and also for retrieving the transaction history for a product.


using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Services
{
    public class InventoryService : IInventoryService
    {

        private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

        private readonly IProductRepository _productRepository;

        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryTransactionRepository inventoryTransactionRepository, IProductRepository productRepository, ILogger<InventoryService> logger)
        {
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _productRepository = productRepository;
            _logger = logger;
        }
        public async Task<bool> AddStockAsync(StockOperationDto stockOperation)
        {
            var productExists = await _productRepository.GetByIdAsync(stockOperation.ProductId);

            if (productExists == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", stockOperation.ProductId);
                throw new NotFoundException("Product not found");
            }

            productExists.AddStock(stockOperation.Quantity);

            var transaction =  new InventoryTransaction(
                stockOperation.ProductId,
                TransactionType.Addition,
                stockOperation.Quantity,
                stockOperation.Notes
            );

            await _inventoryTransactionRepository.AddAsync(transaction);

            await _productRepository.UpdateAsync(productExists);

            //publish event here
            return true;
           
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionHistoryForProductAsync(int productId)
        {
            var productExists = await _productRepository.GetByIdAsync(productId);

            if (productExists == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", productId);
                throw new NotFoundException("Product not found");
            }

            var transactions = await _inventoryTransactionRepository.GetByProductIdAsync(productId);

            return transactions.Select(x => new InventoryTransactionDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Type = x.Type.ToString(),
                Notes = x.Notes,
                ProductName = productExists.Name                
            });
            
        }

        public async Task<bool> WithdrawStockAsync(StockOperationDto stockOperation)
        {
           _logger.LogInformation("Withdrawing stock for product with id {ProductId}", stockOperation.ProductId);

            var productExists = await _productRepository.GetByIdAsync(stockOperation.ProductId);

            if (productExists == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", stockOperation.ProductId);
                throw new NotFoundException("Product not found");
            }

            if (productExists.QuantityInStock < stockOperation.Quantity)
            {
                _logger.LogWarning("Insufficient stock for product with id {ProductId}", stockOperation.ProductId);
                throw new BadRequestException("Insufficient stock");
            }

            productExists.WithdrawStock(stockOperation.Quantity);

            var transaction = new InventoryTransaction(
                stockOperation.ProductId,
                TransactionType.Withdrawal,
                stockOperation.Quantity,
                stockOperation.Notes
            );

            await _inventoryTransactionRepository.AddAsync(transaction);

            await _productRepository.UpdateAsync(productExists);

            //publish event here
            return true;
        }
    }
}