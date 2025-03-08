





using System.Net;
using System.Runtime.CompilerServices;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly ILogger<AuthService> _logger;

        private readonly IUserRepository _userRepository;

        private readonly IJwtService _jwtService;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthService(ILogger<AuthService> logger, IUserRepository userRepository, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<UserDto> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.Items["UserId"] as string;

            if (userId == null)
            {
                _logger.LogWarning("User is not authenticated");
                throw new UnauthorizedException("User is not authenticated");
            }

            var user = await _userRepository.GetByIdAsync(int.Parse(userId));

            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found", userId);
                throw new NotFoundException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto> LoginAsync(AuthRequestDto request)
        {
            _logger.LogInformation("Logging in user with email {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", request.Email);
                throw new NotFoundException("User with this email not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user with email {Email}", request.Email);
                throw new BadRequestException("Invalid password");
            }

            user.UpdateLastLogin();

            await _userRepository.SaveChangesAsync();
            
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddHours(1),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto request)
        {
            _logger.LogInformation("Registering new user with email {Email}", request.Email);

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} already exists", request.Email);
                throw new BadRequestException("User with this email already exists");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User(request.Username,hashedPassword,request.Email,request.Role);

            await _userRepository.AddAsync(newUser);

            newUser.UpdateLastLogin();
            
            await _userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(newUser);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.Now.AddHours(1),
                User = new UserDto
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    Role = newUser.Role
                }
            };



        }
    }
}