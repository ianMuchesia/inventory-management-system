using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Tests.Utilities;
using InventoryManagement.API;
using Xunit;
using InventoryManagement.Domain.Common;

namespace InventoryManagement.Tests.Controllers
{
    public class ReportControllerTests : IClassFixture<TestDatabaseFactory<Program>>
    {
        private readonly HttpClient _client;

        private string? _authToken;

        public ReportControllerTests(TestDatabaseFactory<Program> factory)
        {
            _client = factory.CreateClient();

            // Get authentication token first
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
        public async Task GetInventoryValuationAsync_ShouldReturnInventoryValuation()
        {
            // Arrange
            // Create a product to ensure there is data for valuation
            var newProduct = new CreateProductDto
            {
                Name = "Test Product for Valuation",
                Description = "For inventory valuation testing",
                Category = "Test",
                UnitPrice = 100.00m,
                QuantityInStock = 10,
                ReorderLevel = 5
            };

            await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/inventory-valuation");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryValuationDto>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.TotalValue.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnLowStockProducts()
        {
            // Arrange
            // Create a product with low stock
            var lowStockProduct = new CreateProductDto
            {
                Name = "Low Stock Product",
                Description = "For low stock testing",
                Category = "Test",
                UnitPrice = 50.00m,
                QuantityInStock = 2, // Below reorder level
                ReorderLevel = 5
            };

            await _client.PostAsJsonAsync("/api/v1/products", lowStockProduct);

            // Act
            var response = await _client.GetAsync("/api/v1/reports/low-stock");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ProductDto>>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().Contain(p => p.Name == "Low Stock Product");
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnMatchingProducts()
        {
            // Arrange
            // Create a product to search for
            var searchProduct = new CreateProductDto
            {
                Name = "Searchable Product",
                Description = "For search testing",
                Category = "Test",
                UnitPrice = 75.00m,
                QuantityInStock = 15,
                ReorderLevel = 5
            };

            await _client.PostAsJsonAsync("/api/v1/products", searchProduct);

            var searchRequest = new SearchDto
            {
                SearchTerm = "Searchable",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/reports/search-products", searchRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Items.Should().Contain(p => p.Name == "Searchable Product");
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnEmpty_WhenNoMatchesFound()
        {
            // Arrange
            var searchRequest = new SearchDto
            {
                SearchTerm = "NonExistentProduct",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/reports/search-products", searchRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Items.Should().BeEmpty();
        }
    }
}