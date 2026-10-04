namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;

public interface IKittenClawsController
{
    Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<KittenClawsDto>> GetListAsync(string? limit = null, CancellationToken ct = default);
    Task<KittenClawsDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default);
    Task<KittenClawsDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default);
    Task<DeleteOkObjectResult> DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
