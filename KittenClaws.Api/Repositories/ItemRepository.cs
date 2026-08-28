
namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class KittenClawsRepository : IKittenClawsRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly IFirestoreContext<KittenClaws> _context;

    public KittenClawsRepository(IFirestoreContext<KittenClaws> context)
    {
        _context = context;
    }

    private async Task<KittenClaws> GetItemAsync(string id, CancellationToken ct)
    {
        var item = await _context.GetAsync(id, ct);

        if (item == null || item.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return item;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new KittenClawsDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        return results.Take(DefaultListLimit).Select(item => new KittenClawsDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<KittenClawsDto> CreateAsync(KittenClaws item, CancellationToken ct = default)
    {
        await _context.SetAsync(item.Id, item, ct);

        return new KittenClawsDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<KittenClawsDto> UpdateAsync(KittenClaws item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new KittenClaws
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(updateItem.Id, updateItem, ct);

        return new KittenClawsDto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task<KittenClawsDto> ReplaceAsync(KittenClaws item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new KittenClaws
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(replaceItem.Id, replaceItem, ct);

        return new KittenClawsDto
        {
            Id = replaceItem.Id,
            Name = replaceItem.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;

        await _context.SetAsync(item.Id, item, ct);
    }
}
