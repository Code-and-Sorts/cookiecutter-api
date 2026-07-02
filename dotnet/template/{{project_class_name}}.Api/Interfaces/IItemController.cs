namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;

public interface IItemController
{
    Task<ItemDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<ItemDto>> GetListAsync(CancellationToken ct = default);

    Task<ItemDto> CreateAsync(Stream item, CancellationToken ct = default);

    Task<ItemDto> UpdateAsync(string id, Stream item, CancellationToken ct = default);

    Task<ItemDto> ReplaceAsync(string id, Stream item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
