namespace {{project_class_name}}.Api.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Entities;

public interface IDocumentStore<T> where T : BaseEntity
{
    Task<T?> GetAsync(string id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default);

    Task CreateAsync(T item, CancellationToken ct = default);

    // One read feeds the write, so a store can refuse a write that races another; null when the item is missing.
    Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default);
}
