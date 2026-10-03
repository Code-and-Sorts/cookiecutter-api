namespace KittenClaws.Api.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public interface IDogRepository
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<DogDto>> GetListAsync(int limit, CancellationToken ct = default);

    Task<DogDto> CreateAsync(DogEntity item, string? userId, CancellationToken ct = default);

    Task<DogDto> ReplaceAsync(string id, Action<DogEntity> apply, string? userId, CancellationToken ct = default);

    Task DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
