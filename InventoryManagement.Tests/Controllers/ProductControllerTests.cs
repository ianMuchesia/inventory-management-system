





using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Tests.Utilities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using InventoryManagement.API;
using InventoryManagement.Domain.Common.Responses;

namespace InventoryManagement.Tests.Controllers
{
    public class ProductControllerTests : IClassFixture<TestDatabaseFactory<Program>>
    {
        private readonly HttpClient _client;
        private string? _authToken;


        public ProductControllerTests(TestDatabaseFactory<Program> factory)
        {
            _client = factory.CreateClient();
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
        public async Task CreateProduct_ShouldReturnCreatedProduct()
        {
            // Arrange
            var newProduct = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Category = "Test Category",
                UnitPrice = 15.99m,
                QuantityInStock = 50,
                ReorderLevel = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var createdProduct = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            createdProduct.Should().NotBeNull();
            createdProduct.Data.Name.Should().Be(newProduct.Name);
        }


        [Fact]
        public async Task GetAllProducts_ShouldReturnProductsList()
        {
            // Arrange
            // First create a product to ensure there's at least one to retrieve
            var newProduct = new CreateProductDto
            {
                Name = "Test Product for GetAll",
                Description = "Test Description",
                Category = "Test Category",
                UnitPrice = 15.99m,
                QuantityInStock = 50,
                ReorderLevel = 10
            };
            await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Act
            var response = await _client.GetAsync("/api/v1/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ProductDto>>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
            result.Data.Should().Contain(p => p.Name == "Test Product for GetAll");
        }

        [Fact]
        public async Task GetProductById_ShouldReturnProduct_WhenIdExists()
        {
            // Arrange
            // First create a product to get a known ID
            var newProduct = new CreateProductDto
            {
                Name = "Test Product for GetById",
                Description = "Test Description",
                Category = "Test Category",
                UnitPrice = 25.99m,
                QuantityInStock = 30,
                ReorderLevel = 5
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Act
            var response = await _client.GetAsync($"/api/v1/products/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(productId);
            result.Data.Name.Should().Be("Test Product for GetById");
        }

        [Fact]
        public async Task GetProductById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            int nonExistentProductId = 999999; // Choose a very large ID that's unlikely to exist

            // Act
            var response = await _client.GetAsync($"/api/v1/products/{nonExistentProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            errorResponse.Should().NotBeNull();
            errorResponse.Success.Should().BeFalse();
            errorResponse.Status.Should().Be(404);
            errorResponse.Error.Should().NotBeNull();
            errorResponse.Error.Code.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnUpdatedProduct()
        {
            // Arrange
            // First create a product to update
            var newProduct = new CreateProductDto
            {
                Name = "Original Product",
                Description = "Original Description",
                Category = "Original Category",
                UnitPrice = 10.00m,

                ReorderLevel = 5
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/products", newProduct);
            var createdProduct = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            var productId = createdProduct.Data.Id;

            // Prepare update data
            var updateData = new UpdateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Category = "Updated Category",
                UnitPrice = 15.00m,

                ReorderLevel = 8
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(productId);
            result.Data.Name.Should().Be("Updated Product");
            result.Data.Description.Should().Be("Updated Description");
            result.Data.Category.Should().Be("Updated Category");
            result.Data.UnitPrice.Should().Be(15.00m);

            result.Data.ReorderLevel.Should().Be(8);
        }
    }
}
