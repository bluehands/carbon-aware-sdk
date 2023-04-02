using Microsoft.Extensions.DependencyInjection;
using CarbonAware.DataSources.Memory.Configuration;
using CarbonAware.Model;
using GSF.CarbonAware.Configuration;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CarbonAware.DataSources.Memory;

namespace lib_integration2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var inMemorySettings = new Dictionary<string, string>
            {
                {"LocationDataSourcesConfiguration:LocationSourceFiles:DataFileLocation", ""},
                {"LocationDataSourcesConfiguration:LocationSourceFiles:Prefix", "az"},
                {"LocationDataSourcesConfiguration:LocationSourceFiles:Delimiter", "-"},
                {"DataSources:EmissionsDataSource", ""},
                {"DataSources:ForecastDataSource", "Memory"},
                {"DataSources:Configurations:Memory:Type", "MEMORY"},
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();


            var services = new ServiceCollection()
                .AddLogging(loggerBuilder =>
                {
                    loggerBuilder.ClearProviders();
                    loggerBuilder.AddConsole();
                })
                .AddSingleton<IEmissionsDataProvider, JsonEmissionsDataProvider>()
                .AddForecastServices(configuration)
                .BuildServiceProvider();

                var handler = services.GetService<IForecastHandler>();
            var startDate = new DateTimeOffset(2023, 3, 30, 20, 0, 0, TimeSpan.FromHours(2));
            var endDate = new DateTimeOffset(2023, 3, 31, 19, 0, 0, TimeSpan.FromHours(2));
            var forecast = await handler.GetCurrentForecastAsync(new[] { "DE" }, startDate, endDate, 10);
            var best = forecast.First().OptimalDataPoints.First();


            Console.WriteLine($"Start at {best.Time} with {best.Rating}");
        }
    }

    public class JsonEmissionsDataProvider : IEmissionsDataProvider
    {
        public async Task<List<EmissionsData>> GetForecastData(Location location)
        {
            var json = await File.ReadAllTextAsync("data-sources/json/test-data-wt-de-emissions.json");

            var jsonFile = System.Text.Json.JsonSerializer.Deserialize<EmissionsForecastJsonFile>(json)!;
            return jsonFile.Emissions.Select(e => new EmissionsData()
            {
                Duration = e.Duration,
                Rating = e.Rating,
                Location = location.Name!,
                Time = e.Time
            }).ToList();
        }
    }
}