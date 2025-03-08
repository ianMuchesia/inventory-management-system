


using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/inventories")]
    [Authorize]

    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }


        [HttpPost("add-stock")]
        public async Task<IActionResult> AddStockAsync([FromBody] StockOperationDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started adding stock at {StartTime}", startTime);

                var result = await _inventoryService.AddStockAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed adding stock in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<object>.SuccessNoData("Stock added successfully", 201));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while adding stock: {ex.Message}");
            }
        }

        [HttpPost("withdraw-stock")]
        public async Task<IActionResult> WithdrawStockAsync([FromBody] StockOperationDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started withdrawing stock at {StartTime}", startTime);

                var result = await _inventoryService.WithdrawStockAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed withdrawing stock in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<object>.SuccessNoData("Stock withdrawn successfully", 201));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while withdrawing stock: {ex.Message}");
            }
        }

        [HttpGet("transactions/{productId}")]
        public async Task<IActionResult> GetTransactionHistoryForProductAsync(int productId)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started getting transaction history for product with id {ProductId} at {StartTime}", productId, startTime);

                var result = await _inventoryService.GetTransactionHistoryForProductAsync(productId);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed getting transaction history for product with id {ProductId} in {Duration}", productId, duration.TotalMilliseconds);

                return Ok(ApiResponse<IEnumerable<InventoryTransactionDto>>.SuccessWithData(result, "Transaction history retrieved successfully", 200));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while getting transaction history: {ex.Message}");
            }
        }
    }
}