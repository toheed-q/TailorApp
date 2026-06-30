using TailorApp.Application.Models;
using TailorApp.Domain.Entities;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Use-case service for stitching orders. Validates input, enforces the
/// customer link, and stamps sync metadata. Source of truth for the Orders
/// list and the Home "recent orders" count.
/// </summary>
public interface IOrderService
{
    /// <summary>Creates an order. Throws if the customer is missing or input is invalid.</summary>
    Task<Order> AddAsync(OrderInput input);

    /// <summary>Updates an existing (non-deleted) order. Throws if not found or invalid.</summary>
    Task<Order> UpdateAsync(string id, OrderInput input);

    /// <summary>Soft-deletes an order. Returns false if it does not exist / is already deleted.</summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>Returns a single non-deleted order, or null.</summary>
    Task<Order?> GetByIdAsync(string id);

    /// <summary>All non-deleted orders, newest first.</summary>
    Task<IReadOnlyList<Order>> GetAllAsync();

    /// <summary>Most recent non-deleted orders, newest first.</summary>
    Task<IReadOnlyList<Order>> GetRecentAsync(int take);

    /// <summary>Count of non-deleted orders created within the last <paramref name="days"/> days.</summary>
    Task<int> CountRecentAsync(int days);
}
