using System.Text.Json;

namespace SkyRoutePlatform.API.Features.Flights.Providers;

/// <summary>
/// Implementation of IProviderDataSource that loads JSON data from files.
/// </summary>
public sealed class JsonFileProviderDataSource : IProviderDataSource
{
    private readonly string _filePath;

    /// <summary>
    /// Creates a new instance of JsonFileProviderDataSource.
    /// </summary>
    /// <param name="filePath">The full path to the JSON data file</param>
    public JsonFileProviderDataSource(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Loads and deserializes JSON data from the configured file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into</typeparam>
    /// <returns>Collection of deserialized items</returns>
    public IEnumerable<T> Load<T>()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Provider data file not found: {_filePath}");
        }

        string json = File.ReadAllText(_filePath);

        // NOTE: This option we can receive from instance level and registering in program
        JsonSerializerOptions options = new () { PropertyNameCaseInsensitive = true };

        List<T>? data = JsonSerializer.Deserialize<List<T>>(json, options);

        return data ?? [];
    }
}
