using Infrastructure.Contracts;
using Infrastructure.DataSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Configurations
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddScoped<ISlotRepository>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SlotRepository>>();
                return new SlotRepository(connectionString, logger);
            });

            return services;
        }
    }
}