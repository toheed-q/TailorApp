using System.Globalization;

namespace TailorApp.Components.Shared;

/// <summary>
/// Small presentation helpers shared across components (avatar initials,
/// relative timestamps). Pure functions — no state, easy to reuse.
/// </summary>
public static class DisplayFormat
{
    /// <summary>Up to two uppercase initials from a name (e.g. "Imran Khan" → "IK").</summary>
    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var single = parts[0];
            return (single.Length == 1 ? single : single[..2]).ToUpperInvariant();
        }

        return string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant();
    }

    /// <summary>Coarse "x days ago" style label from a UTC timestamp.</summary>
    public static string RelativeTime(DateTime utc)
    {
        var span = DateTime.UtcNow - (utc.Kind == DateTimeKind.Utc ? utc : utc.ToUniversalTime());
        if (span < TimeSpan.Zero)
            span = TimeSpan.Zero;

        if (span.TotalDays >= 1)
        {
            var days = (int)span.TotalDays;
            return days == 1 ? "1 day ago" : $"{days} days ago";
        }
        if (span.TotalHours >= 1)
        {
            var hours = (int)span.TotalHours;
            return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
        }
        if (span.TotalMinutes >= 1)
        {
            var minutes = (int)span.TotalMinutes;
            return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
        }
        return "just now";
    }

    /// <summary>Formats a whole/decimal number without trailing zeros (for measurements).</summary>
    public static string Number(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);
}
