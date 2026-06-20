using SQLite;

namespace TailorApp.Domain.Entities;

/// <summary>
/// A tailor-shop customer. Owns many <see cref="MeasurementSet"/> records
/// over time (one per order/visit), which is what powers customer history.
/// </summary>
[Table("Customers")]
public class Customer : EntityBase
{
    /// <summary>Customer full name. Indexed for fast search.</summary>
    [Indexed, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Phone number. Indexed for fast search.</summary>
    [Indexed, MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
