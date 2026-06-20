using TailorApp.Domain.Entities;

namespace TailorApp.Application.Models;

/// <summary>
/// Read model pairing a measurement set with its (optional) design record, so
/// history/detail views get everything in one call without doing joins in the UI.
/// </summary>
public sealed class MeasurementSetWithDesign
{
    public required MeasurementSet Measurements { get; init; }
    public DesignPreference? Design { get; init; }
}
