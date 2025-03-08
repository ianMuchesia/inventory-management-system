


using System.Text;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Infrastructure.External;
using InventoryManagement.Infrastructure.Persistence;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {


            // ✅ Ensure the configuration is correctly bound
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtSettings>>().Value);

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is missing in appsettings.json");
            }

            //Database 
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString));







            //Repositories
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddTransient<IUserRepository, UserRepository>();



            //Jwt Authentication
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    // ClockSkew = TimeSpan.Zero
                };
            });


            //this is for the middleware
            services.AddHttpContextAccessor();

            //External Services
            services.AddSingleton<IJwtService, JwtService>();


            return services;
        }
    }
}