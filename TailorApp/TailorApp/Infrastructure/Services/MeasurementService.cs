using TailorApp.Application.Common;
using TailorApp.Application.Exceptions;
using TailorApp.Application.Interfaces;
using TailorApp.Application.Models;
using TailorApp.Domain.Entities;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Services;

/// <summary>
/// SQLite-backed <see cref="IMeasurementService"/>. A measurement set and its
/// design are persisted together inside one transaction for atomicity, and the
/// history query batch-loads designs to avoid N+1.
/// </summary>
public sealed class MeasurementService : IMeasurementService
{
    private readonly IDatabaseService _database;

    public MeasurementService(IDatabaseService database) => _database = database;

    public async Task<MeasurementSetWithDesign> AddAsync(string customerId, MeasurementSetInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();

        // Referential integrity: don't attach measurements to a missing customer.
        var customer = await connection.FindAsync<Customer>(customerId);
        if (customer is null || customer.IsDeleted)
            throw new EntityNotFoundException(nameof(Customer), customerId);

        var now = DateTime.UtcNow;

        var set = new MeasurementSet { CustomerId = customerId };
        ApplyMeasurements(set, input);
        Stamp(set, now, created: true);

        var design = new DesignPreference { MeasurementSetId = set.Id };
        ApplyDesign(design, input.Design);
        Stamp(design, now, created: true);

        await connection.RunInTransactionAsync(tran =>
        {
            tran.Insert(set);
            tran.Insert(design);
        });

        return new MeasurementSetWithDesign { Measurements = set, Design = design };
    }

    public async Task<MeasurementSetWithDesign> UpdateAsync(string measurementSetId, MeasurementSetInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();

        var set = await connection.FindAsync<MeasurementSet>(measurementSetId);
        if (set is null || set.IsDeleted)
            throw new EntityNotFoundException(nameof(MeasurementSet), measurementSetId);

        var now = DateTime.UtcNow;

        ApplyMeasurements(set, input);
        Stamp(set, now, created: false);

        // Find the existing design; create one if (defensively) missing.
        var design = await FindDesignAsync(connection, measurementSetId);
        var designIsNew = design is null;
        design ??= new DesignPreference { MeasurementSetId = set.Id, CreatedAt = now };

        ApplyDesign(design, input.Design);
        Stamp(design, now, created: designIsNew);

        await connection.RunInTransactionAsync(tran =>
        {
            tran.Update(set);
            if (designIsNew)
                tran.Insert(design);
            else
                tran.Update(design);
        });

        return new MeasurementSetWithDesign { Measurements = set, Design = design };
    }

    public async Task<bool> DeleteAsync(string measurementSetId)
    {
        var connection = await _database.GetConnectionAsync();

        var set = await connection.FindAsync<MeasurementSet>(measurementSetId);
        if (set is null || set.IsDeleted)
            return false;

        var now = DateTime.UtcNow;
        set.IsDeleted = true;
        Stamp(set, now, created: false);

        var design = await FindDesignAsync(connection, measurementSetId);
        if (design is not null)
        {
            design.IsDeleted = true;
            Stamp(design, now, created: false);
        }

        await connection.RunInTransactionAsync(tran =>
        {
            tran.Update(set);
            if (design is not null)
                tran.Update(design);
        });

        return true;
    }

    public async Task<MeasurementSetWithDesign?> GetByIdAsync(string measurementSetId)
    {
        var connection = await _database.GetConnectionAsync();

        var set = await connection.FindAsync<MeasurementSet>(measurementSetId);
        if (set is null || set.IsDeleted)
            return null;

        var design = await FindDesignAsync(connection, measurementSetId);
        return new MeasurementSetWithDesign { Measurements = set, Design = design };
    }

    public async Task<IReadOnlyList<MeasurementSetWithDesign>> GetHistoryAsync(string customerId)
    {
        var connection = await _database.GetConnectionAsync();

        var sets = await connection.Table<MeasurementSet>()
            .Where(m => m.CustomerId == customerId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        if (sets.Count == 0)
            return Array.Empty<MeasurementSetWithDesign>();

        // Batch-load all designs for these sets in one query (IN clause) → no N+1.
        var ids = sets.Select(s => s.Id).ToList();
        var designs = await connection.Table<DesignPreference>()
            .Where(d => !d.IsDeleted && ids.Contains(d.MeasurementSetId))
            .ToListAsync();

        var designBySet = designs
            .GroupBy(d => d.MeasurementSetId)
            .ToDictionary(g => g.Key, g => g.First());

        return sets
            .Select(s => new MeasurementSetWithDesign
            {
                Measurements = s,
                Design = designBySet.GetValueOrDefault(s.Id)
            })
            .ToList();
    }

    public async Task<int> CountRecentAsync(int days)
    {
        var connection = await _database.GetConnectionAsync();
        var since = DateTime.UtcNow.AddDays(-Math.Abs(days));
        return await connection.Table<MeasurementSet>()
            .Where(m => !m.IsDeleted && m.CreatedAt >= since)
            .CountAsync();
    }

    private static Task<DesignPreference?> FindDesignAsync(SQLite.SQLiteAsyncConnection connection, string measurementSetId)
        => connection.Table<DesignPreference>()
            .Where(d => d.MeasurementSetId == measurementSetId && !d.IsDeleted)
            .FirstOrDefaultAsync()!;

    private static void ApplyMeasurements(MeasurementSet target, MeasurementSetInput input)
    {
        target.Label = string.IsNullOrWhiteSpace(input.Label) ? null : input.Label.Trim();

        target.KameezLength = input.KameezLength;
        target.Chest = input.Chest;
        target.Waist = input.Waist;
        target.Shoulder = input.Shoulder;
        target.SleeveLength = input.SleeveLength;
        target.Collar = input.Collar;
        target.FrontPocket = input.FrontPocket;
        target.SidePocket = input.SidePocket;

        target.ShalwarLength = input.ShalwarLength;
        target.ShalwarWaist = input.ShalwarWaist;
        target.PanchaWidth = input.PanchaWidth;
        target.ShalwarStyle = input.ShalwarStyle;
    }

    private static void ApplyDesign(DesignPreference target, DesignPreferenceInput input)
    {
        target.CollarDesign = input.CollarDesign;
        target.CuffDesign = input.CuffDesign;
        target.PocketDesign = input.PocketDesign;
        target.ButtonStyle = input.ButtonStyle;
    }

    private static void Stamp(EntityBase entity, DateTime nowUtc, bool created)
    {
        if (created)
            entity.CreatedAt = nowUtc;
        entity.UpdatedAt = nowUtc;
        entity.SyncStatus = SyncStatus.Pending;
    }
}
