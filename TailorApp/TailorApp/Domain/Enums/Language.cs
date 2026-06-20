namespace TailorApp.Domain.Enums;

/// <summary>
/// Supported UI languages. Stored as an int in SQLite and used to drive
/// localization + RTL layout. English is the default (0).
/// </summary>
public enum Language
{
    English = 0,
    Urdu = 1
}
