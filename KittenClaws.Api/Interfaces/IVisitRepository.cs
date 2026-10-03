namespace KittenClaws.Api.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public interface IVisitRepository
{
    Task<VisitDto> GetAsync(string id, CancellationToken ct = default);

    Task<VisitDto> CreateAsync(VisitEntity item, string? userId, CancellationToken ct = default);

    Task<VisitDto> UpdateAsync(string id, Action<VisitEntity> apply, string? userId, CancellationToken ct = default);
}
