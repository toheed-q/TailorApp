using TailorApp.Domain.Enums;

namespace TailorApp.Application.Models;

/// <summary>
/// Input model for the design choices attached to a measurement set.
/// All values default to Unspecified, so design is optional.
/// </summary>
public sealed class DesignPreferenceInput
{
    public CollarDesign CollarDesign { get; set; } = CollarDesign.Unspecified;
    public CuffDesign CuffDesign { get; set; } = CuffDesign.Unspecified;
    public PocketDesign PocketDesign { get; set; } = PocketDesign.Unspecified;
    public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Unspecified;
}
