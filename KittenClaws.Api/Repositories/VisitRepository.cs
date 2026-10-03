namespace KittenClaws.Api.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class VisitRepository(IDocumentStore<VisitEntity> store)
    : EntityRepository<VisitEntity, VisitDto>(store, "Visit"), IVisitRepository
{
    protected override VisitDto ToDto(VisitEntity item) => new(item);

    public Task<VisitDto> GetAsync(string id, CancellationToken ct = default) => GetDtoAsync(id, ct);

    public Task<VisitDto> CreateAsync(VisitEntity item, string? userId, CancellationToken ct = default) => InsertAsync(item, userId, ct);

    public Task<VisitDto> UpdateAsync(string id, Action<VisitEntity> apply, string? userId, CancellationToken ct = default) =>
        MergeAsync(id, apply, userId, ct);
}
