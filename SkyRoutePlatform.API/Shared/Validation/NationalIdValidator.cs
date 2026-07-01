using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Shared.Validation;

/// <summary>
/// Validation strategy for National IDs.
/// Accepts 9-12 alphanumeric characters.
/// </summary>
public sealed class NationalIdValidator : IDocumentValidationStrategy
{
    /// <summary>
    /// Gets the document type this strategy validates.
    /// </summary>
    public DocumentType DocumentType => DocumentType.NationalId;

    /// <summary>
    /// Validates the national ID format.
    /// </summary>
    public string? Validate(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return "National ID is required.";
        }

        if (documentNumber.Length < 9 || documentNumber.Length > 12)
        {
            return "National ID must be between 9 and 12 characters.";
        }

        if (!documentNumber.All(c => char.IsLetterOrDigit(c)))
        {
            return "National ID must contain only letters and numbers.";
        }

        return null;
    }
}
