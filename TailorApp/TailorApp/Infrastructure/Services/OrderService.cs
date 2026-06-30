using TailorApp.Application.Common;
using TailorApp.Application.Exceptions;
using TailorApp.Application.Interfaces;
using TailorApp.Application.Models;
using TailorApp.Domain.Entities;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Services;

/// <summary>
/// SQLite-backed <see cref="IOrderService"/>. Validates input, checks the
/// customer exists, and stamps sync metadata so the row is pushed to Firestore
/// on the next sync.
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly IDatabaseService _database;

    public OrderService(IDatabaseService database) => _database = database;

    public async Task<Order> AddAsync(OrderInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();

        var customer = await connection.FindAsync<Customer>(input.CustomerId);
        if (customer is null || customer.IsDeleted)
            throw new EntityNotFoundException(nameof(Customer), input.CustomerId);

        var now = DateTime.UtcNow;
        var order = new Order { Id = Guid.NewGuid().ToString("N") };
        Apply(order, input);
        order.CreatedAt = now;
        Stamp(order, now);

        await connection.InsertAsync(order);
        return order;
    }

    public async Task<Order> UpdateAsync(string id, OrderInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();

        var order = await connection.FindAsync<Order>(id);
        if (order is null || order.IsDeleted)
            throw new EntityNotFoundException(nameof(Order), id);

        Apply(order, input);
        Stamp(order, DateTime.UtcNow);

        await connection.UpdateAsync(order);
        return order;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var connection = await _database.GetConnectionAsync();

        var order = await connection.FindAsync<Order>(id);
        if (order is null || order.IsDeleted)
            return false;

        order.IsDeleted = true;
        Stamp(order, DateTime.UtcNow);
        await connection.UpdateAsync(order);
        return true;
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        var connection = await _database.GetConnectionAsync();
        var order = await connection.FindAsync<Order>(id);
        return order is null || order.IsDeleted ? null : order;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<Order>()
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int take)
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<Order>()
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(Math.Max(0, take))
            .ToListAsync();
    }

    public async Task<int> CountRecentAsync(int days)
    {
        var connection = await _database.GetConnectionAsync();
        var since = DateTime.UtcNow.AddDays(-Math.Abs(days));
        return await connection.Table<Order>()
            .Where(o => !o.IsDeleted && o.CreatedAt >= since)
            .CountAsync();
    }

    private static void Apply(Order target, OrderInput input)
    {
        target.CustomerId = input.CustomerId;
        target.MeasurementSetId = input.MeasurementSetId;
        target.GarmentType = input.GarmentType;
        target.Quantity = input.Quantity;
        target.Occasion = input.Occasion;
        target.ClothProvider = input.ClothProvider;
        target.FabricType = input.FabricType;
        target.Colour = string.IsNullOrWhiteSpace(input.Colour) ? null : input.Colour.Trim();
        target.ClothReceived = input.ClothReceived;
        target.CollarDesign = input.CollarDesign;
        target.CuffDesign = input.CuffDesign;
        target.OrderDate = input.OrderDate;
        target.DeliveryDate = input.DeliveryDate;
        target.IsUrgent = input.IsUrgent;
        target.StitchingCharges = input.StitchingCharges;
        target.AdvancePaid = input.AdvancePaid;
        target.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
        target.Status = input.Status;
    }

    private static void Stamp(EntityBase entity, DateTime nowUtc)
    {
        entity.UpdatedAt = nowUtc;
        entity.SyncStatus = SyncStatus.Pending;
    }
}
