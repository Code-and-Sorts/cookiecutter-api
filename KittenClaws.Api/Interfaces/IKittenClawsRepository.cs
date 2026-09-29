namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public interface IKittenClawsRepository
{
    Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default);

    Task<KittenClawsDto> CreateAsync(KittenClaws kittenClaws, CancellationToken ct = default);

    Task<KittenClawsDto> UpdateAsync(KittenClaws kittenClaws, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
