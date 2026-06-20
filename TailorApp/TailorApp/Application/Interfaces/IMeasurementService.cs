using TailorApp.Application.Models;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Use-case service for a customer's measurement sets and their design choices.
/// A measurement set and its design are treated as one unit (saved/updated in a
/// single transaction). This is also where "customer history" comes from.
/// </summary>
public interface IMeasurementService
{
    /// <summary>
    /// Adds a new measurement set (+ design) for a customer. Throws if the
    /// customer does not exist / is deleted, or if the input is invalid.
    /// </summary>
    Task<MeasurementSetWithDesign> AddAsync(string customerId, MeasurementSetInput input);

    /// <summary>Updates an existing (non-deleted) measurement set and its design.</summary>
    Task<MeasurementSetWithDesign> UpdateAsync(string measurementSetId, MeasurementSetInput input);

    /// <summary>Soft-deletes a measurement set and its design. Returns false if not found.</summary>
    Task<bool> DeleteAsync(string measurementSetId);

    /// <summary>Returns a single measurement set with its design, or null.</summary>
    Task<MeasurementSetWithDesign?> GetByIdAsync(string measurementSetId);

    /// <summary>
    /// Returns a customer's measurement history (newest first), each paired with
    /// its design. Designs are fetched in a single batched query (no N+1).
    /// </summary>
    Task<IReadOnlyList<MeasurementSetWithDesign>> GetHistoryAsync(string customerId);
}
