using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.Json.Importer;

internal record ElectricityMapsRoot(
    [property: JsonPropertyName("zone")] string Zone,
    [property: JsonPropertyName("forecast")] IReadOnlyList<ElectricityMapsForecast> Forecast,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
);
internal record ElectricityMapsForecast(
    [property: JsonPropertyName("carbonIntensity")] int CarbonIntensity,
    [property: JsonPropertyName("datetime")] DateTime Datetime
);