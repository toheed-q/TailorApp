using TailorApp.Domain.Enums;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Provides localized strings and manages the active language at runtime.
/// Supports live switching (no app restart) and exposes the text direction so
/// the UI can apply RTL for Urdu.
/// </summary>
public interface ILocalizationService
{
    /// <summary>The currently active language.</summary>
    Language CurrentLanguage { get; }

    /// <summary>True when the active language is right-to-left (Urdu).</summary>
    bool IsRtl { get; }

    /// <summary>HTML/CSS direction value: "rtl" or "ltr".</summary>
    string Direction { get; }

    /// <summary>Localized string for a key; returns the key itself if missing.</summary>
    string this[string key] { get; }

    /// <summary>Localized string for a key; returns the key itself if missing.</summary>
    string Get(string key);

    /// <summary>Switches language, persists the choice, and raises <see cref="LanguageChanged"/>.</summary>
    void SetLanguage(Language language);

    /// <summary>Raised after the language changes so components can re-render.</summary>
    event EventHandler? LanguageChanged;

    /// <summary>Loads the saved language preference and applies the culture (call at startup).</summary>
    void Initialize();
}
