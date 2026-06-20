using TailorApp.Application.Models;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Thin REST client over Cloud Firestore, scoped to the signed-in shop's data
/// (<c>shops/{uid}/{collection}</c>). Deals only in neutral field maps; entity
/// mapping lives in the sync layer.
/// </summary>
public interface IFirestoreClient
{
    /// <summary>
    /// Creates or replaces a document by id (PATCH). Field values are CLR types
    /// (string, bool, long, double, DateTime, or null).
    /// </summary>
    Task UpsertAsync(string collection, string documentId,
        IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns documents in a collection whose <c>updatedAt</c> is strictly
    /// greater than <paramref name="sinceUtc"/>, ordered oldest-first.
    /// </summary>
    Task<IReadOnlyList<FirestoreDocument>> GetChangedSinceAsync(string collection,
        DateTime sinceUtc, CancellationToken cancellationToken = default);
}
