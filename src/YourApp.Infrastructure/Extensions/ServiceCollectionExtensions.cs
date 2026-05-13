using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Interfaces;
using YourApp.Infrastructure.Persistence;
using YourApp.Infrastructure.Persistence.Repositories;
using YourApp.Infrastructure.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace YourApp.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext - Postgres (giữ nguyên từ belumi)
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Firebase Init (thay thế JWT auth)
            var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS") 
                               ?? configuration["FIREBASE_CREDENTIALS"];

            if (string.IsNullOrEmpty(firebaseJson))
            {
                throw new InvalidOperationException("FIREBASE_CREDENTIALS is not set in the environment or configuration.");
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(firebaseJson)
            });

            // Memory cache (cho SkinAnalysis)
            services.AddMemoryCache();

            // Skin Analysis Service (giữ nguyên)
            services.AddHttpClient<ISkinAnalysisService, SkinAnalysisService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<ISkinAnalysisService, SkinAnalysisService>();

            return services;
        }
    }
}
