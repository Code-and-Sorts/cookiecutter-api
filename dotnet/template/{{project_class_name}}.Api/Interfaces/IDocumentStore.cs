namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Entities;

public interface IDocumentStore<T> where T : BaseEntity
{
    Task<T?> GetAsync(string id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default);

    Task CreateAsync(T item, CancellationToken ct = default);

    Task SaveAsync(T item, CancellationToken ct = default);
}
