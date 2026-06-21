using SQLite;
using TailorApp.Domain.Enums;

namespace TailorApp.Domain.Entities;

/// <summary>
/// A single dated snapshot of a customer's measurements (one per order/visit).
/// Storing each set rather than overwriting is what gives us customer history.
///
/// Kameez and Shalwar fields are flattened into columns (no child tables) so
/// reads need no joins — faster and lighter on low-end devices. All numeric
/// fields are nullable because not every measurement is always recorded.
/// Units are inches.
/// </summary>
[Table("MeasurementSets")]
public class MeasurementSet : EntityBase
{
    /// <summary>Owning <see cref="Customer"/> id (<see cref="EntityBase.Id"/>). Indexed for history lookups.</summary>
    [Indexed]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>Optional human label, e.g. "Eid order 2026".</summary>
    [MaxLength(120)]
    public string? Label { get; set; }

    // ---- Kameez ----
    public double? KameezLength { get; set; }
    public double? Chest { get; set; }
    public double? Waist { get; set; }
    public double? KameezHip { get; set; }
    public double? Shoulder { get; set; }
    public double? SleeveLength { get; set; }
    public double? ArmWidth { get; set; }
    public double? Collar { get; set; }
    public double? NeckWidth { get; set; }
    public double? Cuff { get; set; }
    public bool FrontPocket { get; set; }
    public bool SidePocket { get; set; }

    // ---- Shalwar ----
    public double? ShalwarLength { get; set; }
    public double? ShalwarWaist { get; set; }
    public double? Hip { get; set; }
    public double? ThighWidth { get; set; }
    public double? PanchaWidth { get; set; }
    public ShalwarStyle ShalwarStyle { get; set; } = ShalwarStyle.Unspecified;
}
