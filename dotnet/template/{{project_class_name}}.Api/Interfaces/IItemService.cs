namespace {{project_class_name}}.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Requests;

public interface IItemService
{
    Task<ItemDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<ItemDto>> GetListAsync(CancellationToken ct = default);

    Task<ItemDto> CreateAsync(CreateItemRequest item, CancellationToken ct = default);

    Task<ItemDto> UpdateAsync(UpdateItemRequest item, CancellationToken ct = default);

    Task<ItemDto> ReplaceAsync(ReplaceItemRequest item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
