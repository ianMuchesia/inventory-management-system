
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Persistence.Interfaces
{
     public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);

        Task SaveChangesAsync();
    }
}