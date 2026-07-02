namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;

public interface IItemRepository
{
    Task<ItemDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<ItemDto>> GetListAsync(CancellationToken ct = default);

    Task<ItemDto> CreateAsync(Item item, CancellationToken ct = default);

    Task<ItemDto> UpdateAsync(Item item, CancellationToken ct = default);

    Task<ItemDto> ReplaceAsync(Item item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
