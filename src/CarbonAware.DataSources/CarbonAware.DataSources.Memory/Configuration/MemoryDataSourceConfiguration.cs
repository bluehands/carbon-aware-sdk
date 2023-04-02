using System.Reflection;
using System.Text.RegularExpressions;
using CarbonAware.Model;

namespace CarbonAware.DataSources.Memory.Configuration;

/// <summary>
/// A configuration class for holding Memory Data config values.
/// </summary>
internal class MemoryDataSourceConfiguration
{
    public IEmissionsDataProvider Provider { get; }

    public MemoryDataSourceConfiguration(IEmissionsDataProvider provider)
    {
        Provider = provider;
    }
}