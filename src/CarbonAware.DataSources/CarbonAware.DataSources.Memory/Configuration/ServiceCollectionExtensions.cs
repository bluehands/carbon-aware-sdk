using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Memory.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryForecastDataSource(this IServiceCollection services)
    {
        services.AddSingleton<MemoryDataSourceConfiguration>();
        services.TryAddSingleton<IForecastDataSource, MemoryDataSource>();
        return services;
    }
}