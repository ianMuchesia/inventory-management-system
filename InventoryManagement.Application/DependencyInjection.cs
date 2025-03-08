








using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IInventoryService, InventoryService>();

            services.AddHttpContextAccessor();
            return services;
        }
    }
}