
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    [Authorize]

    public class ProductController : ControllerBase

    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductAsync([FromBody] CreateProductDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started adding product at {StartTime}", startTime);

                var result = await _productService.CreateProductAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed adding product in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<Product>.SuccessWithData(result, "Product added successfully", 201));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while adding product: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductsAsync()
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started getting all products at {StartTime}", startTime);

                var result = await _productService.GetAllProductsAsync();

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed getting all products in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessWithData(result, "Products retrieved successfully"));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while getting all products: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started getting product with id {ProductId} at {StartTime}", id, startTime);

                var result = await _productService.GetProductByIdAsync(id);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed getting product with id {ProductId} in {Duration}", id, duration.TotalMilliseconds);

                return Ok(ApiResponse<ProductDto>.SuccessWithData(result, "Product retrieved successfully"));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while getting product: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started updating product with id {ProductId} at {StartTime}", id, startTime);

                var result = await _productService.UpdateProductAsync(id, request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed updating product with id {ProductId} in {Duration}", id, duration.TotalMilliseconds);

                return Ok(ApiResponse<ProductDto>.SuccessWithData(result, "Product updated successfully", 201));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while updating product: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started deleting product with id {ProductId} at {StartTime}", id, startTime);

                await _productService.DeleteProductAsync(id);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed deleting product with id {ProductId} in {Duration}", id, duration.TotalMilliseconds);

                return Ok(ApiResponse<object>.SuccessNoData("Product deleted successfully", 204));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while deleting product: {ex.Message}");
            }
        }

       
    }
}