using TailorApp.Application.Common;
using TailorApp.Application.Exceptions;
using TailorApp.Application.Interfaces;
using TailorApp.Application.Models;
using TailorApp.Domain.Entities;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Services;

/// <summary>
/// SQLite-backed <see cref="ICustomerService"/>. Talks to the shared
/// connection directly (no repository), validates input, and stamps the
/// sync metadata required for offline-first Firestore sync.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private readonly IDatabaseService _database;

    public CustomerService(IDatabaseService database) => _database = database;

    public async Task<Customer> AddAsync(CustomerInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();
        var now = DateTime.UtcNow;

        var customer = new Customer
        {
            Name = input.Name.Trim(),
            PhoneNumber = Clean(input.PhoneNumber),
            Address = Clean(input.Address),
            Notes = Clean(input.Notes),
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            SyncStatus = SyncStatus.Pending
        };

        await connection.InsertAsync(customer);
        return customer;
    }

    public async Task<Customer> UpdateAsync(string id, CustomerInput input)
    {
        ModelValidator.Validate(input);

        var connection = await _database.GetConnectionAsync();
        var existing = await connection.FindAsync<Customer>(id);
        if (existing is null || existing.IsDeleted)
            throw new EntityNotFoundException(nameof(Customer), id);

        existing.Name = input.Name.Trim();
        existing.PhoneNumber = Clean(input.PhoneNumber);
        existing.Address = Clean(input.Address);
        existing.Notes = Clean(input.Notes);
        existing.UpdatedAt = DateTime.UtcNow;
        existing.SyncStatus = SyncStatus.Pending;

        await connection.UpdateAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var connection = await _database.GetConnectionAsync();
        var existing = await connection.FindAsync<Customer>(id);
        if (existing is null || existing.IsDeleted)
            return false;

        // Soft delete so the removal can be propagated to the cloud later.
        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.SyncStatus = SyncStatus.Pending;

        await connection.UpdateAsync(existing);
        return true;
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        var connection = await _database.GetConnectionAsync();
        var customer = await connection.FindAsync<Customer>(id);
        return customer is { IsDeleted: false } ? customer : null;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        var connection = await _database.GetConnectionAsync();
        return await connection.Table<Customer>()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        var term = query.Trim();
        var connection = await _database.GetConnectionAsync();

        // sqlite-net translates Contains -> LIKE '%term%' (case-insensitive for
        // ASCII). LIKE against a NULL phone simply yields no match, so no null
        // guard is needed. Runs against the [Indexed] Name/PhoneNumber columns.
        // Null-forgiving: this is an expression tree compiled to SQL by
        // sqlite-net; PhoneNumber is never dereferenced in managed code.
        return await connection.Table<Customer>()
            .Where(c => !c.IsDeleted &&
                        (c.Name.Contains(term) || c.PhoneNumber!.Contains(term)))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>Trims optional text and normalizes empty strings to null.</summary>
    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return value.Trim();
    }
}
