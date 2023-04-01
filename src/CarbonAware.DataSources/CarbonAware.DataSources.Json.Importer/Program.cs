using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CarbonAware.Model;
using CsvHelper;

namespace CarbonAware.DataSources.Json.Importer
{
    internal enum DataSource
    {
        WattTime,
        ElectricityMaps
    }

    internal record CsvRecord(DateTime Timestamp, double Rating);
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
                case DataSource.ElectricityMaps:
                    emissionsForecast = await ImportElectricityMaps(inputFile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            var fileFormat = outputFile.Extension;
            if (fileFormat.Contains("csv", StringComparison.InvariantCultureIgnoreCase))
            {
                var sb = new StringBuilder();
                await using var textWriter = new StringWriter(sb);
                await using var csvWriter = new CsvWriter(textWriter, CultureInfo.CurrentCulture);
                csvWriter.WriteHeader<CsvRecord>();
                await csvWriter.NextRecordAsync();
                await csvWriter.WriteRecordsAsync(emissionsForecast.ForecastData.Select(r => new CsvRecord(r.Time.DateTime, r.Rating)));
                await csvWriter.FlushAsync();
                await File.WriteAllTextAsync(outputFile.FullName, sb.ToString());
            }
            else
            {
                var jsonFile = new EmissionsForecastJsonFile()
                {
                    GeneratedAt = emissionsForecast.GeneratedAt,
                    Emissions = emissionsForecast.ForecastData.Select(d => new EmissionsDataRaw { Time = d.Time, Rating = d.Rating, Duration = d.Duration }).ToList()
                };
                var json = JsonSerializer.Serialize(jsonFile);
                await File.WriteAllTextAsync(outputFile.FullName, json);
            }

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
                Rating = ConvertMoerToGramsPerKilowattHour(j.Value)
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
        private static async Task<EmissionsForecast> ImportElectricityMaps(FileInfo inputFile)
        {
            var json = await File.ReadAllTextAsync(inputFile.FullName);
            var emRoot = JsonSerializer.Deserialize<ElectricityMapsRoot>(json)!;

            var location = emRoot.Zone;
            var emissionDataList = emRoot.Forecast.Select(j => new EmissionsData()
            {
                Time = j.Datetime,
                Location = location,
                Duration = TimeSpan.FromHours(1),
                Rating = j.CarbonIntensity
            }).ToList();
            return new EmissionsForecast
            {
                GeneratedAt = emRoot.UpdatedAt,
                Location = new Location { Name = location },
                ForecastData = emissionDataList
            };
        }
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static double ConvertMoerToGramsPerKilowattHour(double value)
        {
            const double MWH_TO_KWH_CONVERSION_FACTOR = 1000.0;
            const double LBS_TO_GRAMS_CONVERSION_FACTOR = 453.59237;
            return value * LBS_TO_GRAMS_CONVERSION_FACTOR / MWH_TO_KWH_CONVERSION_FACTOR;
        }
    }

}