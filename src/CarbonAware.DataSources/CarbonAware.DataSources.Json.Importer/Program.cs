using System.Text.Json;
using System.Text.Json.Serialization;
using CarbonAware.Model;

namespace CarbonAware.DataSources.Json.Importer
{
    internal enum DataSource
    {
        WattTime
    }
    internal class Program
    {
        static async Task Main(DataSource? source, FileInfo? inputFile, FileInfo? outputFile)
        {
            if (inputFile is not { Exists: true })
            {
                Console.WriteLine("Option --input-file must be set to a valid file");
                return;
            }
            if (outputFile == null)
            {
                Console.WriteLine("Option --output-file must be set");
                return;
            }

            EmissionsForecast emissionsForecast;
            switch (source)
            {
                case DataSource.WattTime:
                    emissionsForecast = await ImportWattTime(inputFile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
            var json = JsonSerializer.Serialize(emissionsForecast.ForecastData);
            await File.WriteAllTextAsync(outputFile.FullName,json);
            Console.WriteLine($"Forecast from {source} for {emissionsForecast.Location.Name} generated at {emissionsForecast.GeneratedAt} with {emissionsForecast.ForecastData.Count()} data points imported.");
        }

        private static async Task<EmissionsForecast> ImportWattTime(FileInfo inputFile)
        {
            var json = await File.ReadAllTextAsync(inputFile.FullName);
            var wattTimeRoot = JsonSerializer.Deserialize<WattTimeRoot>(json)!;

            var emissionDataList = wattTimeRoot.Forecast.Select(j => new EmissionsData()
            {
                Time = j.PointTime,
                Location = j.Ba,
                Duration = TimeSpan.FromMinutes(5),
                Rating = j.Value
            }).ToList();
            var first = emissionDataList.FirstOrDefault();
            var location = first != null ? first.Location : "DE";
            return new EmissionsForecast
            {
                GeneratedAt = wattTimeRoot.GeneratedAt,
                Location = new Location { Name = location },
                ForecastData = emissionDataList
            };
        }
    }

}