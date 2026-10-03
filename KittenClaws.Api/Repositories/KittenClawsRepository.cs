namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class KittenClawsRepository(IDocumentStore<KittenClawsEntity> store)
    : EntityRepository<KittenClawsEntity, KittenClawsDto>(store, "KittenClaws"), IKittenClawsRepository
{
    protected override KittenClawsDto ToDto(KittenClawsEntity item) => new(item);

    public Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => GetDtoAsync(id, ct);

    public Task<IEnumerable<KittenClawsDto>> GetListAsync(int limit, CancellationToken ct = default) => ListAsync(limit, ct);

    public Task<KittenClawsDto> CreateAsync(KittenClawsEntity item, string? userId, CancellationToken ct = default) => InsertAsync(item, userId, ct);

    public Task<KittenClawsDto> UpdateAsync(string id, Action<KittenClawsEntity> apply, string? userId, CancellationToken ct = default) =>
        MergeAsync(id, apply, userId, ct);

    public Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => SoftDeleteAsync(id, userId, ct);
}
