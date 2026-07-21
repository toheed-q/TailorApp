using System.ComponentModel.DataAnnotations;
using TailorApp.Domain.Enums;

namespace TailorApp.Application.Models;

/// <summary>
/// Input model for creating/editing a measurement set together with its design
/// (saved as one unit). All numeric fields are optional inches; when supplied
/// they must be within a sane tailoring range. Bound directly by the Blazor
/// EditForm later.
/// </summary>
public sealed class MeasurementSetInput
{
    [StringLength(120, ErrorMessage = "Label must be 120 characters or fewer.")]
    public string? Label { get; set; }

    // ---- Kameez (inches) ----
    [Range(0, 120, ErrorMessage = "Kameez length must be between 0 and 120.")]
    public double? KameezLength { get; set; }

    [Range(0, 120, ErrorMessage = "Chest must be between 0 and 120.")]
    public double? Chest { get; set; }

    [Range(0, 120, ErrorMessage = "Waist must be between 0 and 120.")]
    public double? Waist { get; set; }

    [Range(0, 120, ErrorMessage = "Shoulder must be between 0 and 120.")]
    public double? Shoulder { get; set; }

    [Range(0, 120, ErrorMessage = "Sleeve length must be between 0 and 120.")]
    public double? SleeveLength { get; set; }

    [Range(0, 120, ErrorMessage = "Collar must be between 0 and 120.")]
    public double? Collar { get; set; }

    public bool FrontPocket { get; set; }

    public bool SidePocket { get; set; }

    // ---- Shalwar (inches) ----
    [Range(0, 120, ErrorMessage = "Shalwar length must be between 0 and 120.")]
    public double? ShalwarLength { get; set; }

    [Range(0, 120, ErrorMessage = "Shalwar waist must be between 0 and 120.")]
    public double? ShalwarWaist { get; set; }

    [Range(0, 120, ErrorMessage = "Thigh width must be between 0 and 120.")]
    public double? ThighWidth { get; set; }

    [Range(0, 120, ErrorMessage = "Pancha width must be between 0 and 120.")]
    public double? PanchaWidth { get; set; }

    public ShalwarStyle ShalwarStyle { get; set; } = ShalwarStyle.Unspecified;

    // ---- Design (saved together with the measurements) ----
    public DesignPreferenceInput Design { get; set; } = new();
}
