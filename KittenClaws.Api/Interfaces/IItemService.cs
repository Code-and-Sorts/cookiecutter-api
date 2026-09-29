namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Requests;

public interface IKittenClawsService
{
    Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default);
    Task<KittenClawsDto> CreateAsync(CreateKittenClawsRequest item, CancellationToken ct = default);
    Task<KittenClawsDto> UpdateAsync(UpdateKittenClawsRequest item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
