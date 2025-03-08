using InventoryManagement.API.Common;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{

    [Route("api/v1/auth")]
    [ApiController]


    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        private readonly ILogger<AuthController> _logger;


        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started registering user at {StartTime}", startTime);

                var result = await _authService.RegisterAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed registering user in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<AuthResponseDto>.SuccessWithData(result, "User registered successfully", 201));


            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {

                throw new InternalServerException($"An error occurred while registering the user: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthRequestDto request)
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started logging in user at {StartTime}", startTime);

                var result = await _authService.LoginAsync(request);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed logging in user in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<AuthResponseDto>.SuccessWithData(result, "User logged in successfully", 200));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while logging in the user: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            try
            {
                var startTime = DateTime.Now;

                _logger.LogInformation("Started fetching current user at {StartTime}", startTime);

                var result = await _authService.GetCurrentUserAsync();

                _logger.LogInformation("The current user is {CurrentUser}", result);

                var duration = DateTime.Now - startTime;

                _logger.LogInformation("Completed fetching current user in {Duration}", duration.TotalMilliseconds);

                return Ok(ApiResponse<UserDto>.SuccessWithData(result, "User fetched successfully", 200));
            }
            catch (ApiException)
            {
                // The middleware will handle this
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InternalServerException($"An error occurred while fetching the current user: {ex.Message}");
            }
        }
    }
}