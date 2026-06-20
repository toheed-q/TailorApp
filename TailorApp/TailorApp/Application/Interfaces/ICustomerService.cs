using TailorApp.Application.Models;
using TailorApp.Domain.Entities;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Use-case service for customers. The UI depends only on this contract.
/// Implementations own validation, persistence and sync-metadata stamping —
/// no repository layer in between.
/// </summary>
public interface ICustomerService
{
    /// <summary>Creates a customer. Throws on validation failure. Returns the persisted entity.</summary>
    Task<Customer> AddAsync(CustomerInput input);

    /// <summary>Updates an existing (non-deleted) customer. Throws if not found or invalid.</summary>
    Task<Customer> UpdateAsync(string id, CustomerInput input);

    /// <summary>Soft-deletes a customer. Returns false if it does not exist / is already deleted.</summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>Returns a single non-deleted customer, or null.</summary>
    Task<Customer?> GetByIdAsync(string id);

    /// <summary>Returns all non-deleted customers, ordered by name.</summary>
    Task<IReadOnlyList<Customer>> GetAllAsync();

    /// <summary>Count of non-deleted customers (efficient COUNT query).</summary>
    Task<int> CountAsync();

    /// <summary>Most recently added non-deleted customers, newest first.</summary>
    Task<IReadOnlyList<Customer>> GetRecentAsync(int take);

    /// <summary>
    /// Searches non-deleted customers by name or phone (case-insensitive contains).
    /// An empty query returns all customers.
    /// </summary>
    Task<IReadOnlyList<Customer>> SearchAsync(string query);
}
