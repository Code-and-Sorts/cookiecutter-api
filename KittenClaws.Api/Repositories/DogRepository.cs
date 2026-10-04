namespace KittenClaws.Api.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class DogRepository(IDocumentStore<DogEntity> store)
    : EntityRepository<DogEntity, DogDto>(store, "Dog"), IDogRepository
{
    protected override void MapFields(DogEntity item, DogDto dto) => dto.Name = item.Name;

    public Task<DogDto> GetAsync(string id, CancellationToken ct = default) => GetDtoAsync(id, ct);

    public Task<IEnumerable<DogDto>> GetListAsync(int limit, CancellationToken ct = default) => ListAsync(limit, ct);

    public Task<DogDto> CreateAsync(DogEntity item, string? userId, CancellationToken ct = default) => InsertAsync(item, userId, ct);

    public Task<DogDto> ReplaceAsync(DogEntity item, string? userId, CancellationToken ct = default) =>
        MergeAsync(item, (current, changes) => current.Name = changes.Name, userId, ct);

    public Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => SoftDeleteAsync(id, userId, ct);
}
