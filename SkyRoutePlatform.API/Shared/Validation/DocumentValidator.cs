using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Shared.Validation;

/// <summary>
/// Service that validates documents for different routes and document types using strategy pattern.
/// Determines document requirements by comparing origin and destination countries.
/// </summary>
public sealed class DocumentValidator : IDocumentValidator
{
    private static readonly Dictionary<string, string> AirportCountries = new()
    {
        { "JFK", "US" },
        { "LAX", "US" },
        { "NYC", "US" },
        { "BOS", "US" },
        { "LHR", "GB" },
        { "CDG", "FR" }
    };

    private readonly Dictionary<DocumentType, IDocumentValidationStrategy> _strategies;

    /// <summary>
    /// Creates a new instance of DocumentValidator.
    /// </summary>
    public DocumentValidator()
    {
        _strategies = new Dictionary<DocumentType, IDocumentValidationStrategy>
        {
            { DocumentType.PassportNumber, new PassportNumberValidator() },
            { DocumentType.NationalId, new NationalIdValidator() }
        };
    }

    /// <summary>
    /// Determines if a route is international based on airport country codes.
    /// </summary>
    private static bool IsInternationalRoute(string originCode, string destinationCode)
    {
        if (!AirportCountries.TryGetValue(originCode, out string? originCountry))
        {
            return true; // Assume international if origin country unknown
        }

        if (!AirportCountries.TryGetValue(destinationCode, out string? destCountry))
        {
            return true; // Assume international if destination country unknown
        }

        return !originCountry.Equals(destCountry, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates document details for the given route.
    /// Applies route-based document type requirements and uses appropriate validation strategy.
    /// </summary>
    public string? ValidateDocumentForRoute(DocumentType documentType, string documentNumber, string originCode, string destinationCode)
    {
        bool isInternational = IsInternationalRoute(originCode, destinationCode);

        // Enforce route-based document type requirements
        string? validationError = documentType switch
        {
            _ when isInternational && documentType != DocumentType.PassportNumber =>
                "International routes require a Passport Number.",
            _ when !isInternational && documentType != DocumentType.NationalId =>
                "Domestic routes require a National ID.",
            _ => null
        };

        if (validationError is not null)
        {
            return validationError;
        }

        // Apply document type-specific validation using strategy
        if (_strategies.TryGetValue(documentType, out var strategy))
        {
            return strategy.Validate(documentNumber);
        }

        return "Unsupported document type.";
    }
}
