namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Requests;

public interface IDogService
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<DogDto>> GetListAsync(int limit, CancellationToken ct = default);
    Task<DogDto> CreateAsync(CreateDogRequest item, string? userId, CancellationToken ct = default);
    Task<DogDto> ReplaceAsync(ReplaceDogRequest item, string? userId, CancellationToken ct = default);
    Task DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
