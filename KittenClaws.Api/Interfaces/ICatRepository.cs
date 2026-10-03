namespace KittenClaws.Api.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public interface ICatRepository
{
    Task<CatDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<CatDto>> GetListAsync(int limit, CancellationToken ct = default);

    Task<CatDto> CreateAsync(CatEntity item, string? userId, CancellationToken ct = default);

    Task<CatDto> UpdateAsync(string id, Action<CatEntity> apply, string? userId, CancellationToken ct = default);

    Task DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
