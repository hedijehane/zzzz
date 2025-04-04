// Intranet.Infrastructure/DependencyInjection/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Intranet.Infrastructure.Data.Repositories;    // For UserRepository

namespace Intranet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}