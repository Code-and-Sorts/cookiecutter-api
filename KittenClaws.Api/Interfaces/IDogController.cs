namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;

public interface IDogController
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<DogDto>> GetListAsync(string? limit = null, CancellationToken ct = default);
    Task<DogDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default);
    Task<DogDto> ReplaceAsync(string id, Stream item, string? userId, CancellationToken ct = default);
    Task<DeleteOkObjectResult> DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
