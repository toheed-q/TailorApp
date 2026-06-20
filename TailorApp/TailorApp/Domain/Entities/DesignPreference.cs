using SQLite;
using TailorApp.Domain.Enums;

namespace TailorApp.Domain.Entities;

/// <summary>
/// Design choices tied to a specific <see cref="MeasurementSet"/> (1:1).
/// Kept as a separate table/document because preferences change per order
/// and map cleanly to a Firestore sub-document, keeping the measurement row lean.
/// </summary>
[Table("DesignPreferences")]
public class DesignPreference : EntityBase
{
    /// <summary>Owning <see cref="MeasurementSet"/> id (<see cref="EntityBase.Id"/>). Indexed for lookups.</summary>
    [Indexed]
    public string MeasurementSetId { get; set; } = string.Empty;

    public CollarDesign CollarDesign { get; set; } = CollarDesign.Unspecified;
    public CuffDesign CuffDesign { get; set; } = CuffDesign.Unspecified;
    public PocketDesign PocketDesign { get; set; } = PocketDesign.Unspecified;
    public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Unspecified;
}
