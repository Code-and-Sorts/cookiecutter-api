namespace {{cookiecutter.project_class_name}}.Api.Repositories;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using {{cookiecutter.project_class_name}}.Api.Interfaces;

public class FirestoreContext<T> : IFirestoreContext<T> where T : class
{
    private readonly CollectionReference _collection;

    public FirestoreContext(FirestoreDb firestoreDb, string collectionName)
    {
        _collection = firestoreDb.Collection(collectionName);
    }

    public async Task<T?> GetAsync(string id, CancellationToken ct = default)
    {
        var snapshot = await _collection.Document(id).GetSnapshotAsync(ct);
        if (!snapshot.Exists)
        {
            return null;
        }
        return snapshot.ConvertTo<T>();
    }

    public async Task<IReadOnlyList<T>> GetListAsync(string field, object value, CancellationToken ct = default)
    {
        var snapshot = await _collection.WhereEqualTo(field, value).GetSnapshotAsync(ct);
        return snapshot.Documents.Select(doc => doc.ConvertTo<T>()).ToList();
    }

    public async Task SetAsync(string id, T item, CancellationToken ct = default)
    {
        await _collection.Document(id).SetAsync(item, cancellationToken: ct);
    }
}
