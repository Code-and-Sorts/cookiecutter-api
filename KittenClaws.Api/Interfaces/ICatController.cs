namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;

public interface ICatController
{
    Task<CatDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<CatDto>> GetListAsync(string? limit = null, CancellationToken ct = default);
    Task<CatDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default);
    Task<CatDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default);
    Task<DeleteOkObjectResult> DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
