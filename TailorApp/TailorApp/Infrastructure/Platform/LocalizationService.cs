using System.Globalization;
using System.Resources;
using Microsoft.Maui.Storage;
using TailorApp.Application.Interfaces;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Platform;

/// <summary>
/// <see cref="ILocalizationService"/> backed by a <see cref="ResourceManager"/>
/// over the AppStrings resx files. Switching language swaps the thread culture
/// and raises an event; the choice is persisted in <see cref="Preferences"/>.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private const string PreferenceKey = "app_language";

    private static readonly ResourceManager Resources = new(
        "TailorApp.Localization.AppStrings",
        typeof(LocalizationService).Assembly);

    private static readonly CultureInfo English = new("en");
    private static readonly CultureInfo Urdu = new("ur");

    public Language CurrentLanguage { get; private set; } = Language.English;

    public bool IsRtl => CurrentLanguage == Language.Urdu;

    public string Direction => IsRtl ? "rtl" : "ltr";

    public event EventHandler? LanguageChanged;

    public string this[string key] => Get(key);

    public string Get(string key)
        => Resources.GetString(key, CultureFor(CurrentLanguage)) ?? key;

    public void Initialize()
    {
        var saved = Preferences.Default.Get(PreferenceKey, (int)Language.English);
        ApplyCulture(Enum.IsDefined(typeof(Language), saved) ? (Language)saved : Language.English);
    }

    public void SetLanguage(Language language)
    {
        if (language == CurrentLanguage)
            return;

        ApplyCulture(language);
        Preferences.Default.Set(PreferenceKey, (int)language);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyCulture(Language language)
    {
        CurrentLanguage = language;

        var culture = CultureFor(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static CultureInfo CultureFor(Language language)
        => language == Language.Urdu ? Urdu : English;
}
