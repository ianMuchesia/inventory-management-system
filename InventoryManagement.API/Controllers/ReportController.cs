

using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    [Authorize]

    public class ReportController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        private readonly IProductService _productService;

        private readonly ILogger<InventoryController> _logger;

        public ReportController(IInventoryService inventoryService, ILogger<InventoryController> logger, IProductService productService)
        {
            _inventoryService = inventoryService;
            _logger = logger;
            _productService = productService;
        }

        [HttpGet("inventory-valuation")]
        public async Task<IActionResult> GetInventoryValuationAsync()
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started getting inventory valuation at {StartTime}", startTime);

                var result = await _productService.GetInventoryValuationAsync();

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed getting inventory valuation in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<InventoryValuationDto>.SuccessWithData(result));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while getting inventory valuation: {ex.Message}");
            }
        }




        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProductsAsync()
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started getting low stock products at {StartTime}", startTime);

                var result = await _productService.GetLowStockProductsAsync();

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed getting low stock products in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessWithData(result));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while getting low stock products: {ex.Message}");
            }
        }


        [HttpPost("search-products")]
        public async Task<IActionResult> SearchProductsAsync([FromBody] SearchDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started searching products at {StartTime}", startTime);

                var result = await _productService.SearchProductsAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed searching products in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessWithData(result));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while searching products: {ex.Message}");
            }
        }
    }
}