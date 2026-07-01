namespace SkyRoutePlatform.API.Shared.Models;

/// <summary>
/// Represents the cabin class types available for flight bookings.
/// </summary>
public enum CabinClass
{
    Economy,
    Business,
    FirstClass
}

/// <summary>
/// Represents the types of documents that can be used for passenger identification.
/// </summary>
public enum DocumentType
{
    PassportNumber,
    NationalId
}

/// <summary>
/// Represents the airline providers available in the system.
/// </summary>
public enum ProviderType
{
    GlobalAir,
    BudgetWings
}
