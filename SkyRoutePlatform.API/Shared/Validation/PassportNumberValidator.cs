using SkyRoutePlatform.API.Shared.Models;

namespace SkyRoutePlatform.API.Shared.Validation;

/// <summary>
/// Validation strategy for Passport Numbers.
/// Accepts 6-9 alphanumeric characters.
/// </summary>
public sealed class PassportNumberValidator : IDocumentValidationStrategy
{
    /// <summary>
    /// Gets the document type this strategy validates.
    /// </summary>
    public DocumentType DocumentType => DocumentType.PassportNumber;

    /// <summary>
    /// Validates the passport number format.
    /// </summary>
    public string? Validate(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return "Passport number is required.";
        }

        if (documentNumber.Length < 6 || documentNumber.Length > 9)
        {
            return "Passport number must be between 6 and 9 characters.";
        }

        if (!documentNumber.All(c => char.IsLetterOrDigit(c)))
        {
            return "Passport number must contain only letters and numbers.";
        }

        return null;
    }
}
