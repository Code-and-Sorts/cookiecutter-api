namespace {{project_class_name}}.Api.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<ItemDto> GetAsync(string id, CancellationToken ct = default) => await _itemRepository.GetAsync(id, ct);

    public async Task<IEnumerable<ItemDto>> GetListAsync(CancellationToken ct = default) => await _itemRepository.GetListAsync(ct);

    public async Task<ItemDto> CreateAsync(CreateItemRequest item, CancellationToken ct = default)
    {
        var newItem = new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = item.Name,
            CreatedBy = item.CreatedBy,
            UpdatedBy = item.UpdatedBy,
        };
        return await _itemRepository.CreateAsync(newItem, ct);
    }

    public async Task<ItemDto> UpdateAsync(UpdateItemRequest item, CancellationToken ct = default)
    {
        var updatedItem = new Item
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _itemRepository.UpdateAsync(updatedItem, ct);
    }

    public async Task<ItemDto> ReplaceAsync(ReplaceItemRequest item, CancellationToken ct = default)
    {
        var replacedItem = new Item
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _itemRepository.ReplaceAsync(replacedItem, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _itemRepository.DeleteAsync(id, ct);
}
