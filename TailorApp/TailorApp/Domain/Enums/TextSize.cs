namespace TailorApp.Domain.Enums;

/// <summary>
/// App-wide text scale, for older shop owners who want larger, easier-to-read
/// text. Applied as a root font-size percentage so all rem-based sizing scales.
/// </summary>
public enum TextSize
{
    Small = 0,
    Normal = 1,
    Large = 2
}
