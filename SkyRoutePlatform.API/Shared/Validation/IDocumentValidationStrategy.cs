using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Shared.Validation;

/// <summary>
/// Strategy interface for validating specific document types.
/// </summary>
public interface IDocumentValidationStrategy
{
    /// <summary>
    /// Gets the document type this strategy validates.
    /// </summary>
    DocumentType DocumentType { get; }

    /// <summary>
    /// Validates the document number for this document type.
    /// </summary>
    /// <param name="documentNumber">The document number to validate</param>
    /// <returns>Error message if invalid; null if valid</returns>
    string? Validate(string documentNumber);
}
