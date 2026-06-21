using Microsoft.Maui.Storage;
using TailorApp.Application.Interfaces;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Platform;

/// <summary>
/// <see cref="IAppSettingsService"/> backed by <see cref="Preferences"/>. Each
/// setter persists immediately and raises <see cref="Changed"/> only when the
/// value actually changes, so listeners don't re-render needlessly.
/// </summary>
public sealed class AppSettingsService : IAppSettingsService
{
    private const string UnitKey = "settings_unit";
    private const string StyleKey = "settings_default_shalwar_style";
    private const string TextSizeKey = "settings_text_size";
    private const string ShopNameKey = "settings_shop_name";
    private const string ShopTaglineKey = "settings_shop_tagline";
    private const string ShopCityKey = "settings_shop_city";
    private const string LastBackupKey = "settings_last_backup_utc";

    public event EventHandler? Changed;

    public MeasurementUnit Unit
    {
        get => (MeasurementUnit)Preferences.Default.Get(UnitKey, (int)MeasurementUnit.Inches);
        set => SetEnum(UnitKey, Unit, value);
    }

    public ShalwarStyle DefaultShalwarStyle
    {
        get => (ShalwarStyle)Preferences.Default.Get(StyleKey, (int)ShalwarStyle.Simple);
        set => SetEnum(StyleKey, DefaultShalwarStyle, value);
    }

    public TextSize TextSize
    {
        get => (TextSize)Preferences.Default.Get(TextSizeKey, (int)TextSize.Normal);
        set => SetEnum(TextSizeKey, TextSize, value);
    }

    public string ShopName
    {
        get => Preferences.Default.Get(ShopNameKey, "Darzi");
        set => SetString(ShopNameKey, ShopName, value);
    }

    public string ShopTagline
    {
        get => Preferences.Default.Get(ShopTaglineKey, "Tailor Studio");
        set => SetString(ShopTaglineKey, ShopTagline, value);
    }

    public string ShopCity
    {
        get => Preferences.Default.Get(ShopCityKey, string.Empty);
        set => SetString(ShopCityKey, ShopCity, value);
    }

    public DateTime? LastBackupUtc
    {
        get
        {
            var ticks = Preferences.Default.Get(LastBackupKey, 0L);
            return ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);
        }
        set
        {
            var ticks = value?.ToUniversalTime().Ticks ?? 0L;
            if (ticks == Preferences.Default.Get(LastBackupKey, 0L))
                return;
            Preferences.Default.Set(LastBackupKey, ticks);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetEnum<T>(string key, T current, T value) where T : Enum
    {
        if (current.Equals(value))
            return;
        Preferences.Default.Set(key, Convert.ToInt32(value));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SetString(string key, string current, string value)
    {
        value = (value ?? string.Empty).Trim();
        if (current == value)
            return;
        Preferences.Default.Set(key, value);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
