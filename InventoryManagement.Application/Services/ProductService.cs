
// Summary: This file contains the implementation of the IProductService interface.
// It is responsible for handling the business logic for the product management system.


using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Services
{
    public class ProductService : IProductService
    {

        private readonly IProductRepository _productRespository;

        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRespository, ILogger<ProductService> logger)
        {
            _productRespository = productRespository;
            _logger = logger;
        }
        public async Task<Product> CreateProductAsync(CreateProductDto productDto)
        {
            _logger.LogInformation("Creating product {@ProductDto}", productDto);

            var productExists = await _productRespository.GetByNameAsync(productDto.Name);

            if (productExists != null)
            {
                _logger.LogWarning("Product with name {ProductName} already exists", productDto.Name);
                throw new BadRequestException("Product with name already exists");
            }

            var newProduct = new Product(productDto.Name, productDto.Description, productDto.Category, productDto.UnitPrice, productDto.QuantityInStock, productDto.ReorderLevel);

            await _productRespository.AddAsync(newProduct);

            _logger.LogInformation("Product created {@Product}", newProduct);


            return newProduct;
        }

        public async Task DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with id {ProductId}", id);

            var productExists = await _productRespository.GetByIdAsync(id);

            if (productExists == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", id);
                throw new NotFoundException("Product not found");
            }

            await _productRespository.DeleteAsync(productExists);

            _logger.LogInformation("Product with id {ProductId} deleted", id);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");

            var products = await _productRespository.GetAllAsync();

            _logger.LogInformation("Got all products");

            return products.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Category = x.Category,
                UnitPrice = x.UnitPrice,
                QuantityInStock = x.QuantityInStock,
                ReorderLevel = x.ReorderLevel
            });
        }

        public async Task<InventoryValuationDto> GetInventoryValuationAsync()
        {
            _logger.LogInformation("Getting inventory valuation");

            var products = await _productRespository.GetAllAsync();

            var productValuations = products.Select(p => new ProductValuationDto
            {
                ProductId = p.Id,
                Name = p.Name,
                QuantityInStock = p.QuantityInStock,
                UnitPrice = p.UnitPrice,
                TotalValue = p.QuantityInStock * p.UnitPrice
            }).ToList();

            return new InventoryValuationDto
            {
                TotalProducts = productValuations.Count,
                TotalItems = productValuations.Sum(p => p.QuantityInStock),
                TotalValue = productValuations.Sum(p => p.TotalValue),
                Products = productValuations
            };
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            _logger.LogInformation("Getting low stock products");

            var products = await _productRespository.GetLowStockProductsAsync();

            _logger.LogInformation("Got low stock products");

            return products.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Category = x.Category,
                UnitPrice = x.UnitPrice,
                QuantityInStock = x.QuantityInStock,
                ReorderLevel = x.ReorderLevel
            });

        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRespository.GetByIdAsync(id);

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                UnitPrice = product.UnitPrice,
                QuantityInStock = product.QuantityInStock,
                ReorderLevel = product.ReorderLevel
            };
        }

        public async Task<PagedResult<ProductDto>> SearchProductsAsync(SearchDto searchDto)
        {
            var productsQuery = _productRespository.AllProductsQeury();

            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchDto.SearchTerm) || p.Description.Contains(searchDto.SearchTerm));
            }

            if (!string.IsNullOrEmpty(searchDto.CategoryName))
            {
                productsQuery = productsQuery.Where(p => p.Category == searchDto.CategoryName);
            }


            // Apply sorting

            if (!string.IsNullOrEmpty(searchDto.SortBy))
            {
                switch (searchDto.SortBy)
                {
                    case "name":
                        productsQuery = !searchDto.SortDescending ? productsQuery.OrderBy(p => p.Name) : productsQuery.OrderByDescending(p => p.Name);
                        break;
                    case "category":
                        productsQuery = !searchDto.SortDescending ? productsQuery.OrderBy(p => p.Category) : productsQuery.OrderByDescending(p => p.Category);
                        break;
                    case "unitPrice":
                        productsQuery = !searchDto.SortDescending ? productsQuery.OrderBy(p => p.UnitPrice) : productsQuery.OrderByDescending(p => p.UnitPrice);
                        break;
                    case "quantityInStock":
                        productsQuery = !searchDto.SortDescending ? productsQuery.OrderBy(p => p.QuantityInStock) : productsQuery.OrderByDescending(p => p.QuantityInStock);
                        break;
                    case "reorderLevel":
                        productsQuery = !searchDto.SortDescending ? productsQuery.OrderBy(p => p.ReorderLevel) : productsQuery.OrderByDescending(p => p.ReorderLevel);
                        break;
                    default:
                        productsQuery = productsQuery.OrderBy(p => p.Name);
                        break;
                }



            }

            //paginate 
            var products = productsQuery.Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    UnitPrice = p.UnitPrice,
                    QuantityInStock = p.QuantityInStock,
                    ReorderLevel = p.ReorderLevel
                });

            var totalProducts = await productsQuery.CountAsync();

            return new PagedResult<ProductDto>
            {
                Items = products,
                TotalCount = totalProducts,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };



        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            _logger.LogInformation("Updating product with id {ProductId} {@ProductDto}", id, productDto);

            var product = await _productRespository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", id);
                throw new NotFoundException("Product not found");
            }

            //check if product with name already exists
            var productExists = await _productRespository.GetByNameAsync(productDto.Name);

            if (productExists != null && productExists.Id != id)
            {
                _logger.LogWarning("Product with name {ProductName} already exists", productDto.Name);
                throw new BadRequestException("Product with name already exists");
            }

            product.Update(productDto.Name, productDto.Description, productDto.Category, productDto.UnitPrice, productDto.ReorderLevel);

            await _productRespository.UpdateAsync(product);

            _logger.LogInformation("Product with id {ProductId} updated", id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                UnitPrice = product.UnitPrice,
                QuantityInStock = product.QuantityInStock,
                ReorderLevel = product.ReorderLevel
            };
        }
    }
}