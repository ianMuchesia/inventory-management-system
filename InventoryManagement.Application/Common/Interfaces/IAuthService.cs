



using InventoryManagement.Application.DTOs;

namespace InventoryManagement.Application.Common.Interfaces
{
     public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto request);
        Task<AuthResponseDto> LoginAsync(AuthRequestDto request);
        Task<UserDto> GetCurrentUserAsync();
    }
}