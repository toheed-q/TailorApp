using TailorApp.Application.Models;
using TailorApp.Domain.Entities;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Sync;

/// <summary>
/// Explicit (no-reflection) mappers between domain entities and the neutral
/// Firestore field maps. Sync metadata that is purely local (SyncStatus,
/// SyncedAt) is NOT stored in the cloud; only createdAt/updatedAt/isDeleted
/// travel, since they drive last-writer-wins and soft-delete propagation.
/// </summary>
internal static class CloudMappers
{
    // ---- Customer ----

    public static Dictionary<string, object?> ToFields(Customer c) => new()
    {
        ["name"] = c.Name,
        ["phoneNumber"] = c.PhoneNumber,
        ["address"] = c.Address,
        ["notes"] = c.Notes,
        ["createdAt"] = c.CreatedAt,
        ["updatedAt"] = c.UpdatedAt,
        ["isDeleted"] = c.IsDeleted
    };

    public static Customer ToCustomer(FirestoreDocument doc)
    {
        var f = doc.Fields;
        return new Customer
        {
            Id = doc.Id,
            Name = GetString(f, "name") ?? string.Empty,
            PhoneNumber = GetString(f, "phoneNumber"),
            Address = GetString(f, "address"),
            Notes = GetString(f, "notes"),
            CreatedAt = GetDateTime(f, "createdAt"),
            UpdatedAt = GetDateTime(f, "updatedAt"),
            IsDeleted = GetBool(f, "isDeleted")
        };
    }

    // ---- MeasurementSet ----

    public static Dictionary<string, object?> ToFields(MeasurementSet m) => new()
    {
        ["customerId"] = m.CustomerId,
        ["label"] = m.Label,
        ["kameezLength"] = m.KameezLength,
        ["chest"] = m.Chest,
        ["waist"] = m.Waist,
        ["shoulder"] = m.Shoulder,
        ["sleeveLength"] = m.SleeveLength,
        ["collar"] = m.Collar,
        ["neckWidth"] = m.NeckWidth,
        ["cuff"] = m.Cuff,
        ["shalwarLength"] = m.ShalwarLength,
        ["shalwarWaist"] = m.ShalwarWaist,
        ["hip"] = m.Hip,
        ["panchaWidth"] = m.PanchaWidth,
        ["shalwarStyle"] = (int)m.ShalwarStyle,
        ["createdAt"] = m.CreatedAt,
        ["updatedAt"] = m.UpdatedAt,
        ["isDeleted"] = m.IsDeleted
    };

    public static MeasurementSet ToMeasurementSet(FirestoreDocument doc)
    {
        var f = doc.Fields;
        return new MeasurementSet
        {
            Id = doc.Id,
            CustomerId = GetString(f, "customerId") ?? string.Empty,
            Label = GetString(f, "label"),
            KameezLength = GetDoubleN(f, "kameezLength"),
            Chest = GetDoubleN(f, "chest"),
            Waist = GetDoubleN(f, "waist"),
            Shoulder = GetDoubleN(f, "shoulder"),
            SleeveLength = GetDoubleN(f, "sleeveLength"),
            Collar = GetDoubleN(f, "collar"),
            NeckWidth = GetDoubleN(f, "neckWidth"),
            Cuff = GetDoubleN(f, "cuff"),
            ShalwarLength = GetDoubleN(f, "shalwarLength"),
            ShalwarWaist = GetDoubleN(f, "shalwarWaist"),
            Hip = GetDoubleN(f, "hip"),
            PanchaWidth = GetDoubleN(f, "panchaWidth"),
            ShalwarStyle = GetEnum<ShalwarStyle>(f, "shalwarStyle"),
            CreatedAt = GetDateTime(f, "createdAt"),
            UpdatedAt = GetDateTime(f, "updatedAt"),
            IsDeleted = GetBool(f, "isDeleted")
        };
    }

    // ---- DesignPreference ----

    public static Dictionary<string, object?> ToFields(DesignPreference d) => new()
    {
        ["measurementSetId"] = d.MeasurementSetId,
        ["collarDesign"] = (int)d.CollarDesign,
        ["cuffDesign"] = (int)d.CuffDesign,
        ["pocketDesign"] = (int)d.PocketDesign,
        ["buttonStyle"] = (int)d.ButtonStyle,
        ["createdAt"] = d.CreatedAt,
        ["updatedAt"] = d.UpdatedAt,
        ["isDeleted"] = d.IsDeleted
    };

    public static DesignPreference ToDesignPreference(FirestoreDocument doc)
    {
        var f = doc.Fields;
        return new DesignPreference
        {
            Id = doc.Id,
            MeasurementSetId = GetString(f, "measurementSetId") ?? string.Empty,
            CollarDesign = GetEnum<CollarDesign>(f, "collarDesign"),
            CuffDesign = GetEnum<CuffDesign>(f, "cuffDesign"),
            PocketDesign = GetEnum<PocketDesign>(f, "pocketDesign"),
            ButtonStyle = GetEnum<ButtonStyle>(f, "buttonStyle"),
            CreatedAt = GetDateTime(f, "createdAt"),
            UpdatedAt = GetDateTime(f, "updatedAt"),
            IsDeleted = GetBool(f, "isDeleted")
        };
    }

    // ---- Field readers (defensive about numeric type drift) ----

    private static string? GetString(IReadOnlyDictionary<string, object?> f, string key)
        => f.TryGetValue(key, out var v) ? v as string : null;

    private static bool GetBool(IReadOnlyDictionary<string, object?> f, string key)
        => f.TryGetValue(key, out var v) && v is bool b && b;

    private static DateTime GetDateTime(IReadOnlyDictionary<string, object?> f, string key)
        => f.TryGetValue(key, out var v) && v is DateTime dt ? dt : default;

    private static double? GetDoubleN(IReadOnlyDictionary<string, object?> f, string key)
    {
        if (!f.TryGetValue(key, out var v) || v is null)
            return null;
        return v switch
        {
            double d => d,
            long l => l,
            int i => i,
            _ => null
        };
    }

    private static T GetEnum<T>(IReadOnlyDictionary<string, object?> f, string key) where T : struct, Enum
    {
        if (f.TryGetValue(key, out var v) && v is not null)
        {
            var number = v switch { long l => l, int i => i, double d => (long)d, _ => 0L };
            if (Enum.IsDefined(typeof(T), (int)number))
                return (T)Enum.ToObject(typeof(T), (int)number);
        }
        return default;
    }
}
