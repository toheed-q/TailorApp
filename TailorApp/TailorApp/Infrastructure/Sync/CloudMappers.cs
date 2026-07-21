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
        ["frontPocket"] = m.FrontPocket,
        ["sidePocket"] = m.SidePocket,
        ["shalwarLength"] = m.ShalwarLength,
        ["shalwarWaist"] = m.ShalwarWaist,
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
            FrontPocket = GetBool(f, "frontPocket"),
            SidePocket = GetBool(f, "sidePocket"),
            ShalwarLength = GetDoubleN(f, "shalwarLength"),
            ShalwarWaist = GetDoubleN(f, "shalwarWaist"),
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

    // ---- Order ----

    public static Dictionary<string, object?> ToFields(Order o) => new()
    {
        ["customerId"] = o.CustomerId,
        ["measurementSetId"] = o.MeasurementSetId,
        ["garmentType"] = (int)o.GarmentType,
        ["quantity"] = o.Quantity,
        ["occasion"] = (int)o.Occasion,
        ["clothProvider"] = (int)o.ClothProvider,
        ["fabricType"] = (int)o.FabricType,
        ["colour"] = o.Colour,
        ["clothReceived"] = o.ClothReceived,
        ["collarDesign"] = (int)o.CollarDesign,
        ["cuffDesign"] = (int)o.CuffDesign,
        ["orderDate"] = o.OrderDate,
        ["deliveryDate"] = o.DeliveryDate,
        ["isUrgent"] = o.IsUrgent,
        ["stitchingCharges"] = o.StitchingCharges,
        ["advancePaid"] = o.AdvancePaid,
        ["balanceDue"] = o.BalanceDue,
        ["notes"] = o.Notes,
        ["status"] = (int)o.Status,
        ["createdAt"] = o.CreatedAt,
        ["updatedAt"] = o.UpdatedAt,
        ["isDeleted"] = o.IsDeleted
    };

    public static Order ToOrder(FirestoreDocument doc)
    {
        var f = doc.Fields;
        return new Order
        {
            Id = doc.Id,
            CustomerId = GetString(f, "customerId") ?? string.Empty,
            MeasurementSetId = GetString(f, "measurementSetId"),
            GarmentType = GetEnum<GarmentType>(f, "garmentType"),
            Quantity = GetInt(f, "quantity"),
            Occasion = GetEnum<Occasion>(f, "occasion"),
            ClothProvider = GetEnum<ClothProvider>(f, "clothProvider"),
            FabricType = GetEnum<FabricType>(f, "fabricType"),
            Colour = GetString(f, "colour"),
            ClothReceived = GetBool(f, "clothReceived"),
            CollarDesign = GetEnum<CollarDesign>(f, "collarDesign"),
            CuffDesign = GetEnum<CuffDesign>(f, "cuffDesign"),
            OrderDate = GetDateTime(f, "orderDate"),
            DeliveryDate = GetDateTime(f, "deliveryDate"),
            IsUrgent = GetBool(f, "isUrgent"),
            StitchingCharges = GetDoubleN(f, "stitchingCharges") ?? 0,
            AdvancePaid = GetDoubleN(f, "advancePaid") ?? 0,
            Notes = GetString(f, "notes"),
            Status = GetEnum<OrderStatus>(f, "status"),
            CreatedAt = GetDateTime(f, "createdAt"),
            UpdatedAt = GetDateTime(f, "updatedAt"),
            IsDeleted = GetBool(f, "isDeleted")
        };
    }

    // ---- Field readers (defensive about numeric type drift) ----

    private static int GetInt(IReadOnlyDictionary<string, object?> f, string key)
        => (int)(GetDoubleN(f, key) ?? 0);


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
