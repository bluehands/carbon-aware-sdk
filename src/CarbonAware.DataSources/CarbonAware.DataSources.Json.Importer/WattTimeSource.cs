using System.Text.Json.Serialization;

namespace CarbonAware.DataSources.Json.Importer;

internal record WattTimeRoot(
    [property: JsonPropertyName("generated_at")] DateTime GeneratedAt,
    [property: JsonPropertyName("forecast")] IReadOnlyList<WattTimeForecast> Forecast
);
internal record WattTimeForecast(
    [property: JsonPropertyName("point_time")] DateTime PointTime,
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("ba")] string Ba
);