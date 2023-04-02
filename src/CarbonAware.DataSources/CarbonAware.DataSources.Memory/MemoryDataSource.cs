using System.Text.Json;
using CarbonAware.DataSources.Memory.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarbonAware.DataSources.Memory;

/// <summary>
/// Represents a JSON data source.
/// </summary>
internal class MemoryDataSource : IForecastDataSource
{
    public string Name => "MemoryDataSource";

    public string Description => "Plugin to read data from memory for Carbon Aware SDK";

    public string Author => "bluehands";

    public string Version => "0.0.1";

    public double MinSamplingWindow => 1440;  // 24 hrs


    private readonly ILogger<MemoryDataSource> _logger;
    private MemoryDataSourceConfiguration _configuration;

    /// <summary>
    /// Creates a new instance of the <see cref="MemoryDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public MemoryDataSource(ILogger<MemoryDataSource> logger, MemoryDataSourceConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        return await GetCarbonIntensityForecastAsync(location, DateTimeOffset.Now);
    }

    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        var emissionsData = await _configuration.Provider.GetForecastData(location);
        if (!emissionsData.Any())
        {
            _logger.LogDebug("Emission data list is empty");
            return new EmissionsForecast();
        }
        _logger.LogDebug($"Total emission records retrieved {emissionsData.Count()}");


        return new EmissionsForecast()
        {
            Location = location,
            GeneratedAt = DateTimeOffset.Now,
            ForecastData = emissionsData,
            OptimalDataPoints = CarbonAwareOptimalEmission.GetOptimalEmissions(emissionsData)
        };
    }

}
