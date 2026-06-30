using System.ComponentModel.DataAnnotations;
using TailorApp.Domain.Enums;

namespace TailorApp.Application.Models;

/// <summary>
/// Input model for creating/editing an <see cref="Domain.Entities.Order"/>.
/// Carries the same validation the service enforces, so it can drive the
/// Blazor EditForm and the service-side validator alike.
/// </summary>
public sealed class OrderInput
{
    [Required(ErrorMessage = "Select a customer for this order.")]
    public string CustomerId { get; set; } = string.Empty;

    public string? MeasurementSetId { get; set; }

    public GarmentType GarmentType { get; set; } = GarmentType.FullSuit;

    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; } = 1;

    public Occasion Occasion { get; set; } = Occasion.Unspecified;

    public ClothProvider ClothProvider { get; set; } = ClothProvider.CustomerBrought;

    public FabricType FabricType { get; set; } = FabricType.Unspecified;

    [StringLength(60, ErrorMessage = "Colour must be 60 characters or fewer.")]
    public string? Colour { get; set; }

    public bool ClothReceived { get; set; }

    public CollarDesign CollarDesign { get; set; } = CollarDesign.Unspecified;

    public CuffDesign CuffDesign { get; set; } = CuffDesign.Unspecified;

    public DateTime OrderDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public bool IsUrgent { get; set; }

    [Range(0, 99_999_999, ErrorMessage = "Stitching charges are out of range.")]
    public double StitchingCharges { get; set; }

    [Range(0, 99_999_999, ErrorMessage = "Advance is out of range.")]
    public double AdvancePaid { get; set; }

    [StringLength(500, ErrorMessage = "Notes must be 500 characters or fewer.")]
    public string? Notes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}
