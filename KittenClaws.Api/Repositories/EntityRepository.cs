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

    protected async Task<TDto> MergeAsync(string id, Action<TEntity> apply, string? userId, CancellationToken ct) =>
        ToDto(await UpdateLiveAsync(id, apply, userId, ct));

    protected Task SoftDeleteAsync(string id, string? userId, CancellationToken ct) =>
        UpdateLiveAsync(id, current => current.IsDeleted = true, userId, ct);

    // A write without a user id removes updatedBy, so it always names the latest writer.
    private async Task<TEntity> UpdateLiveAsync(string id, Action<TEntity> change, string? userId, CancellationToken ct) =>
        await store.UpdateAsync(id, current =>
        {
            if (current.IsDeleted)
            {
                throw new NotFoundException(resourceName, id);
            }
            change(current);
            current.UpdatedBy = userId;
            current.UpdatedTimestamp = Timestamps.Now();
        }, ct) ?? throw new NotFoundException(resourceName, id);

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
