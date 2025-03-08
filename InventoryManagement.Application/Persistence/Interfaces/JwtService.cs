using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Persistence.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}