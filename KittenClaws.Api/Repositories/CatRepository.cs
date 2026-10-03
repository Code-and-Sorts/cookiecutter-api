namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class CatRepository(IDocumentStore<CatEntity> store)
    : EntityRepository<CatEntity, CatDto>(store, "Cat"), ICatRepository
{
    protected override CatDto ToDto(CatEntity item) => new(item);

    public Task<CatDto> GetAsync(string id, CancellationToken ct = default) => GetDtoAsync(id, ct);

    public Task<IEnumerable<CatDto>> GetListAsync(int limit, CancellationToken ct = default) => ListAsync(limit, ct);

    public Task<CatDto> CreateAsync(CatEntity item, string? userId, CancellationToken ct = default) => InsertAsync(item, userId, ct);

    public Task<CatDto> UpdateAsync(string id, Action<CatEntity> apply, string? userId, CancellationToken ct = default) =>
        MergeAsync(id, apply, userId, ct);

    public Task<CatDto> ReplaceAsync(string id, Action<CatEntity> apply, string? userId, CancellationToken ct = default) =>
        MergeAsync(id, apply, userId, ct);

    public Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => SoftDeleteAsync(id, userId, ct);
}
