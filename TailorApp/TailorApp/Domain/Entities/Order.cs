using SQLite;
using TailorApp.Domain.Enums;

namespace TailorApp.Domain.Entities;

/// <summary>
/// A stitching order for a customer. References the customer's saved
/// measurements (by id) and captures the garment, cloth, design overrides,
/// schedule and payment for this specific job. Amounts are in the shop's
/// local currency (whole units).
/// </summary>
[Table("Orders")]
public class Order : EntityBase
{
    /// <summary>Owning <see cref="Customer"/> id. Indexed for per-customer lookups.</summary>
    [Indexed]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>The <see cref="MeasurementSet"/> used for this order, if any.</summary>
    public string? MeasurementSetId { get; set; }

    // ---- Order ----
    public GarmentType GarmentType { get; set; } = GarmentType.FullSuit;
    public int Quantity { get; set; } = 1;
    public Occasion Occasion { get; set; } = Occasion.Unspecified;

    // ---- Cloth ----
    public ClothProvider ClothProvider { get; set; } = ClothProvider.CustomerBrought;
    public FabricType FabricType { get; set; } = FabricType.Unspecified;
    [MaxLength(60)]
    public string? Colour { get; set; }
    public bool ClothReceived { get; set; }

    // ---- Design (defaults from the customer's profile, overridable per order) ----
    public CollarDesign CollarDesign { get; set; } = CollarDesign.Unspecified;
    public CuffDesign CuffDesign { get; set; } = CuffDesign.Unspecified;

    // ---- Schedule ----
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public bool IsUrgent { get; set; }

    // ---- Payment ----
    public double StitchingCharges { get; set; }
    public double AdvancePaid { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Remaining amount owed (never negative). Not persisted.</summary>
    [Ignore]
    public double BalanceDue => Math.Max(0, StitchingCharges - AdvancePaid);
}
