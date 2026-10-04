namespace KittenClaws.Api.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;

public interface IDocumentStore<T> where T : BaseEntity
{
    Task<T?> GetAsync(string id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default);

    Task CreateAsync(T item, CancellationToken ct = default);

    Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default);
}
