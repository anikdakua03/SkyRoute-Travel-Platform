using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Shared.Validation;

/// <summary>
/// Main interface for document validation that combines route checking and strategy selection.
/// </summary>
public interface IDocumentValidator
{
    /// <summary>
    /// Validates document details for the given route using appropriate strategy.
    /// </summary>
    /// <param name="documentType">The type of document provided</param>
    /// <param name="documentNumber">The document number/value</param>
    /// <param name="originCode">Origin airport code</param>
    /// <param name="destinationCode">Destination airport code</param>
    /// <returns>Validation result with error message if invalid; null if valid</returns>
    string? ValidateDocumentForRoute(DocumentType documentType, string documentNumber, string originCode, string destinationCode);
}
