
namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using Microsoft.Azure.Cosmos;

public class CatRepository : ICatRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly Container _container;

    public CatRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    private async Task<Cat> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<Cat>(id, new PartitionKey(id), null, ct);
        var item = response.Resource;

        if (item.IsDeleted)
        {
            throw new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0);
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
        var query = _container.GetItemQueryIterator<Cat>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<Cat>();

        while (query.HasMoreResults && results.Count < DefaultListLimit)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        return results.Take(DefaultListLimit).Select(item => new CatDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<CatDto> CreateAsync(Cat item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new CatDto
        {
            Id = created.Id,
            Name = created.Name,
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
        var response = await _container.ReplaceItemAsync<Cat>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new CatDto
        {
            Id = updated.Id,
            Name = updated.Name,
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
        var response = await _container.ReplaceItemAsync<Cat>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
        var replaced = response.Resource;
        return new CatDto
        {
            Id = replaced.Id,
            Name = replaced.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;
        await _container.ReplaceItemAsync<Cat>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
}

public class DogRepository : IDogRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly Container _container;

    public DogRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    private async Task<Dog> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<Dog>(id, new PartitionKey(id), null, ct);
        var item = response.Resource;

        if (item.IsDeleted)
        {
            throw new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0);
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
        var query = _container.GetItemQueryIterator<Dog>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<Dog>();

        while (query.HasMoreResults && results.Count < DefaultListLimit)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        return results.Take(DefaultListLimit).Select(item => new DogDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<DogDto> CreateAsync(Dog item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new DogDto
        {
            Id = created.Id,
            Name = created.Name,
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
        var response = await _container.ReplaceItemAsync<Dog>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new DogDto
        {
            Id = updated.Id,
            Name = updated.Name,
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
        var response = await _container.ReplaceItemAsync<Dog>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
        var replaced = response.Resource;
        return new DogDto
        {
            Id = replaced.Id,
            Name = replaced.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;
        await _container.ReplaceItemAsync<Dog>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
}
