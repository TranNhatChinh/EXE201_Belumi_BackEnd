using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using YourApp.Application.Common.Behaviors;

namespace YourApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            // MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Mapperly automatic registration using Scrutor
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Mapper")))
                .AsSelf()
                .WithSingletonLifetime());

            return services;
        }
    }
}
