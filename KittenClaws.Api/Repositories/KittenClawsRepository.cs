namespace KittenClaws.Api.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class KittenClawsRepository(IDocumentStore<KittenClawsEntity> store)
    : EntityRepository<KittenClawsEntity, KittenClawsDto>(store, "KittenClaws"), IKittenClawsRepository
{
    protected override void MapFields(KittenClawsEntity item, KittenClawsDto dto) => dto.Name = item.Name;

    public Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => GetDtoAsync(id, ct);

    public Task<IEnumerable<KittenClawsDto>> GetListAsync(int limit, CancellationToken ct = default) => ListAsync(limit, ct);

    public Task<KittenClawsDto> CreateAsync(KittenClawsEntity item, string? userId, CancellationToken ct = default) => InsertAsync(item, userId, ct);

    public Task<KittenClawsDto> UpdateAsync(KittenClawsEntity item, string? userId, CancellationToken ct = default) =>
        MergeAsync(item, (current, changes) => current.Name = changes.Name ?? current.Name, userId, ct);

    public Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => SoftDeleteAsync(id, userId, ct);
}
