using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Tests.Utilities;
using InventoryManagement.API;
using Xunit;

namespace InventoryManagement.Tests.Controllers
{
    public class InventoryControllerTests : IClassFixture<TestDatabaseFactory<Program>>
    {
        private readonly HttpClient _client;

        private string? _authToken;
        public InventoryControllerTests(TestDatabaseFactory<Program> factory)
        {
            _client = factory.CreateClient();

            //Get authentication token first
            SetupAuthenticationAsync().Wait();

        }

        private async Task SetupAuthenticationAsync()
        {
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            try
            {
                // Try to login with the test user
                var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    // Register the test user if login fails
                    var registerRequest = new
                    {
                        Username = "testuser",
                        Email = "test@example.com",
                        Password = "TestPassword123!"
                    };

                    var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

                    // Check if registration was successful
                    if (!registerResponse.IsSuccessStatusCode)
                    {
                        var registerError = await registerResponse.Content.ReadAsStringAsync();
                        throw new InvalidOperationException($"Failed to register test user: {registerError}");
                    }

                    // Try logging in again after registration
                    loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

                    if (!loginResponse.IsSuccessStatusCode)
                    {
                        var loginError = await loginResponse.Content.ReadAsStringAsync();
                        throw new InvalidOperationException($"Failed to login after registration: {loginError}");
                    }
                }

                // Get the response content as string first for debugging
                var responseContent = await loginResponse.Content.ReadAsStringAsync();

                // Now try to deserialize
                var authResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();

                if (authResponse == null || authResponse.Data == null)
                {
                    throw new InvalidOperationException($"Invalid auth response format: {responseContent}");
                }

                _authToken = authResponse.Data.Token;

                if (string.IsNullOrEmpty(_authToken))
                {
                    throw new InvalidOperationException("Auth token is null or empty");
                }

                // Set the token for all subsequent requests
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to setup authentication for tests: {ex.Message}", ex);
            }
        }

        [Fact]
        public async Task AddStock_ShouldReturnSuccess()
        {
            // Arrange
            // First create a product that we'll add stock to
            var newProduct = new CreateProductDto
            {
                Name = "Stock Test Product",
                Description = "For inventory testing",
                Category = "Test",
                UnitPrice = 19.99m,
                QuantityInStock = 10,
                ReorderLevel = 5
            };

            var createProductResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Create stock operation request
            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 15,
                Notes = "Test stock addition"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/inventories/add-stock", stockOperation);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Stock added successfully");
        }

        [Fact]
        public async Task WithdrawStock_ShouldReturnSuccess()
        {
            // Arrange
            // First create a product with stock
            var newProduct = new CreateProductDto
            {
                Name = "Withdrawal Test Product",
                Description = "For inventory testing",
                Category = "Test",
                UnitPrice = 29.99m,
                QuantityInStock = 30, // Starting with sufficient stock
                ReorderLevel = 5
            };

            var createProductResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Create withdrawal operation request
            var withdrawalOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 10,
                Notes = "Test stock withdrawal"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/inventories/withdraw-stock", withdrawalOperation);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Stock withdrawn successfully");
        }

        [Fact]
        public async Task WithdrawStock_ShouldReturnError_WhenInsufficientStock()
        {
            // Arrange
            // First create a product with limited stock
            var newProduct = new CreateProductDto
            {
                Name = "Limited Stock Product",
                Description = "For testing insufficient stock",
                Category = "Test",
                UnitPrice = 39.99m,
                QuantityInStock = 5, // Starting with limited stock
                ReorderLevel = 3
            };

            var createProductResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Try to withdraw more than available
            var withdrawalOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 10, // More than available
                Notes = "This should fail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/inventories/withdraw-stock", withdrawalOperation);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNull();
            // Check for specific error message about insufficient stock
            result.Error.Detail.Should().Contain("Insufficient stock");
        }

        [Fact]
        public async Task GetTransactionHistory_ShouldReturnTransactions()
        {
            // Arrange
            // First create a product
            var newProduct = new CreateProductDto
            {
                Name = "Transaction History Test",
                Description = "For testing transaction history",
                Category = "Test",
                UnitPrice = 49.99m,
                QuantityInStock = 20,
                ReorderLevel = 5
            };

            var createProductResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Add stock to generate a transaction
            var stockOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 10,
                Notes = "Add stock transaction"
            };

            await _client.PostAsJsonAsync("/api/v1/inventories/add-stock", stockOperation);

            // Withdraw stock to generate another transaction
            var withdrawOperation = new StockOperationDto
            {
                ProductId = productId,
                Quantity = 5,
                Notes = "Withdraw stock transaction"
            };

            await _client.PostAsJsonAsync("/api/v1/inventories/withdraw-stock", withdrawOperation);

            // Act
            var response = await _client.GetAsync($"/api/v1/inventories/transactions/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<InventoryTransactionDto>>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(2);

            // Verify transaction details
            result.Data.Should().Contain(t => t.ProductId == productId && t.Quantity == 10);
            result.Data.Should().Contain(t => t.ProductId == productId && t.Quantity == 5); // Negative for withdrawal
        }

        [Fact]
        public async Task GetTransactionHistory_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            int nonExistentProductId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/v1/inventories/transactions/{nonExistentProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            errorResponse.Should().NotBeNull();
            errorResponse.Success.Should().BeFalse();
            errorResponse.Status.Should().Be(404);
        }
    }
}