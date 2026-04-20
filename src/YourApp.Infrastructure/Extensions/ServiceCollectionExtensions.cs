using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Domain.Entities;
using YourApp.Infrastructure.Persistence;
using YourApp.Infrastructure.Persistence.Repositories;

namespace YourApp.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext (using in-memory db for demonstration or sqlite/sqlserver, assuming SQL Server for now)
            // You can replace UseSqlServer with your preferred provider.
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            
            // Register PasswordHasher
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }
    }
}
