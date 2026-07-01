namespace SkyRoutePlatform.API.Features.Flights.Providers;

/// <summary>
/// Abstraction for loading provider-specific data from persistent sources.
/// </summary>
public interface IProviderDataSource
{
    /// <summary>
    /// Loads provider data synchronously from the data source.
    /// </summary>
    /// <typeparam name="T">The type of data to deserialize</typeparam>
    /// <returns>Collection of loaded data items</returns>
    IEnumerable<T> Load<T>();
}
