namespace TailorApp.Domain.Enums;

/// <summary>
/// Unit measurements are displayed/entered in. Stored values are always inches
/// (the entity's canonical unit); this only affects presentation and input.
/// </summary>
public enum MeasurementUnit
{
    Inches = 0,
    Centimeters = 1
}
