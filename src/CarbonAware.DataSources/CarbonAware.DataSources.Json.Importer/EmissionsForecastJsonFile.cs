namespace CarbonAware.DataSources.Json.Importer;

internal class EmissionsForecastJsonFile
{
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.Now;
    public List<EmissionsDataRaw> Emissions { get; set; } = new List<EmissionsDataRaw>();
}
internal record EmissionsDataRaw
{
    public DateTimeOffset Time { get; set; }
    public double Rating { get; set; }
    public TimeSpan Duration { get; set; }
}

