namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Requests;

public interface IKittenClawsService
{
    Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<KittenClawsDto>> GetListAsync(int limit, CancellationToken ct = default);
    Task<KittenClawsDto> CreateAsync(CreateKittenClawsRequest item, string? userId, CancellationToken ct = default);
    Task<KittenClawsDto> UpdateAsync(UpdateKittenClawsRequest item, string? userId, CancellationToken ct = default);
    Task DeleteAsync(string id, string? userId, CancellationToken ct = default);
}
