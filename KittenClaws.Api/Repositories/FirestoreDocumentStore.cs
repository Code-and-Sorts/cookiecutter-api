namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class FirestoreDocumentStore<T>(FirestoreDb firestoreDb, string collectionName) : IDocumentStore<T> where T : BaseEntity
{
    private readonly CollectionReference _collection = firestoreDb.Collection(collectionName);

    public async Task<T?> GetAsync(string id, CancellationToken ct = default)
    {
        var snapshot = await _collection.Document(id).GetSnapshotAsync(ct);
        return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
    }

    public async Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default)
    {
        var snapshot = await _collection.WhereEqualTo("isDeleted", false).Limit(limit).GetSnapshotAsync(ct);
        return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
    }

    public Task CreateAsync(T item, CancellationToken ct = default) => SaveAsync(item, ct);

    public async Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default)
    {
        var item = await GetAsync(id, ct);
        if (item != null)
        {
            change(item);
            await SaveAsync(item, ct);
        }
        return item;
    }

    // An explicit field map, so unset createdBy/updatedBy are not stored as null.
    public Task SaveAsync(T item, CancellationToken ct = default) =>
        _collection.Document(item.Id).SetAsync(item.ToDocument(), cancellationToken: ct);
}
