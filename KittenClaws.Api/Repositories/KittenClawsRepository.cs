
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
        var kittenClaws = await _context.GetAsync(id, ct);

        if (kittenClaws == null || kittenClaws.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return kittenClaws;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct)
    {
        var kittenClaws = await GetItemAsync(id, ct);

        return new KittenClawsDto
        {
            Id = kittenClaws.Id,
            Name = kittenClaws.Name,
        };
    }

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        var kittenClawsDtos = results.Take(DefaultListLimit).Select(kittenClaws => new KittenClawsDto
        {
            Id = kittenClaws.Id,
            Name = kittenClaws.Name,
        });

        return kittenClawsDtos;
    }

    public async Task<KittenClawsDto> CreateAsync(KittenClaws kittenClaws, CancellationToken ct)
    {
        await _context.SetAsync(kittenClaws.Id, kittenClaws, ct);

        return new KittenClawsDto
        {
            Id = kittenClaws.Id,
            Name = kittenClaws.Name,
        };
    }

    public async Task<KittenClawsDto> UpdateAsync(KittenClaws item, CancellationToken ct)
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

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;

        await _context.SetAsync(item.Id, item, ct);
    }
}
