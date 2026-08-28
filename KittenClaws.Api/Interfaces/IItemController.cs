namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;

public interface IKittenClawsController
{
    Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default);
    Task<KittenClawsDto> CreateAsync(Stream item, CancellationToken ct = default);
    Task<KittenClawsDto> UpdateAsync(string id, Stream item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
