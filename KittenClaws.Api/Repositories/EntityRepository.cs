namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public abstract class EntityRepository<TEntity, TDto>(IDocumentStore<TEntity> store, string resourceName)
    where TEntity : BaseEntity
{
    protected abstract TDto ToDto(TEntity item);

    protected async Task<TDto> GetDtoAsync(string id, CancellationToken ct) => ToDto(await GetLiveAsync(id, ct));

    protected async Task<IEnumerable<TDto>> ListAsync(int limit, CancellationToken ct)
    {
        var items = await store.GetLiveListAsync(limit, ct);
        return items.Take(limit).Select(ToDto).ToList();
    }

    protected async Task<TDto> InsertAsync(TEntity item, string? userId, CancellationToken ct)
    {
        var now = Timestamps.Now();
        item.Id = ItemIds.New();
        item.IsDeleted = false;
        item.CreatedTimestamp = now;
        item.UpdatedTimestamp = now;
        item.CreatedBy = userId;
        item.UpdatedBy = userId;
        await store.CreateAsync(item, ct);
        return ToDto(item);
    }

    protected async Task<TDto> MergeAsync(TEntity changes, Action<TEntity, TEntity> applyFields, string? userId, CancellationToken ct)
    {
        var current = await GetLiveAsync(changes.Id, ct);
        applyFields(current, changes);
        // Stores write the whole record, so a null user id drops a stale updatedBy.
        current.UpdatedBy = userId;
        current.UpdatedTimestamp = Timestamps.Now();
        await store.SaveAsync(current, ct);
        return ToDto(current);
    }

    protected async Task SoftDeleteAsync(string id, string? userId, CancellationToken ct)
    {
        var current = await GetLiveAsync(id, ct);
        current.IsDeleted = true;
        current.UpdatedBy = userId;
        current.UpdatedTimestamp = Timestamps.Now();
        await store.SaveAsync(current, ct);
    }

    private async Task<TEntity> GetLiveAsync(string id, CancellationToken ct)
    {
        var item = await store.GetAsync(id, ct);
        if (item == null || item.IsDeleted)
        {
            throw new NotFoundException(resourceName, id);
        }
        return item;
    }
}
