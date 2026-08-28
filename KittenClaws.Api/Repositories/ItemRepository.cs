
namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class CatRepository : ICatRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly IFirestoreContext<Cat> _context;

    public CatRepository(IFirestoreContext<Cat> context)
    {
        _context = context;
    }

    private async Task<Cat> GetItemAsync(string id, CancellationToken ct)
    {
        var item = await _context.GetAsync(id, ct);

        if (item == null || item.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return item;
    }

    public async Task<CatDto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new CatDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        return results.Take(DefaultListLimit).Select(item => new CatDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<CatDto> CreateAsync(Cat item, CancellationToken ct = default)
    {
        await _context.SetAsync(item.Id, item, ct);

        return new CatDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<CatDto> UpdateAsync(Cat item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new Cat
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(updateItem.Id, updateItem, ct);

        return new CatDto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task<CatDto> ReplaceAsync(Cat item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new Cat
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(replaceItem.Id, replaceItem, ct);

        return new CatDto
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

public class DogRepository : IDogRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly IFirestoreContext<Dog> _context;

    public DogRepository(IFirestoreContext<Dog> context)
    {
        _context = context;
    }

    private async Task<Dog> GetItemAsync(string id, CancellationToken ct)
    {
        var item = await _context.GetAsync(id, ct);

        if (item == null || item.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return item;
    }

    public async Task<DogDto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new DogDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        return results.Take(DefaultListLimit).Select(item => new DogDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<DogDto> CreateAsync(Dog item, CancellationToken ct = default)
    {
        await _context.SetAsync(item.Id, item, ct);

        return new DogDto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<DogDto> UpdateAsync(Dog item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new Dog
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(updateItem.Id, updateItem, ct);

        return new DogDto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task<DogDto> ReplaceAsync(Dog item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new Dog
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(replaceItem.Id, replaceItem, ct);

        return new DogDto
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
