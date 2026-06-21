using TailorApp.Domain.Enums;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Persisted user/shop preferences (backed by platform Preferences). Setting any
/// property saves it immediately and raises <see cref="Changed"/> so the UI can
/// react. Language is handled separately by <see cref="ILocalizationService"/>.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>Display/entry unit for measurements (stored values stay in inches).</summary>
    MeasurementUnit Unit { get; set; }

    /// <summary>Shalwar style pre-selected on a new measurement set.</summary>
    ShalwarStyle DefaultShalwarStyle { get; set; }

    /// <summary>App-wide text scale.</summary>
    TextSize TextSize { get; set; }

    /// <summary>Shop display name (shown on Home and the brand card).</summary>
    string ShopName { get; set; }

    /// <summary>Shop tagline/studio name.</summary>
    string ShopTagline { get; set; }

    /// <summary>Shop city (optional).</summary>
    string ShopCity { get; set; }

    /// <summary>UTC time of the last successful cloud backup, if any.</summary>
    DateTime? LastBackupUtc { get; set; }

    /// <summary>Raised whenever any setting changes.</summary>
    event EventHandler? Changed;
}
