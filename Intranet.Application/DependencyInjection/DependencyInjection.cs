// Intranet.Application/DependencyInjection/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using Intranet.Application.Services;
using Intranet.Application.Interfaces;
using Intranet.Application.Common.Behaviors;

namespace Intranet.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 1. MediatR with all handlers and behaviors
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                // Add pipeline behaviors (order matters)
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            // 2. FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // 3. Application Services
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ITokenService, JwtTokenService>(); // If implementing JWT

            // 4. Caching (optional)
            services.AddMemoryCache();

            // 5. Configure MediatR options
            services.Configure<MediatRServiceConfiguration>(cfg =>
            {
                cfg.Lifetime = ServiceLifetime.Scoped;
            });

            return services;
        }
    }
}