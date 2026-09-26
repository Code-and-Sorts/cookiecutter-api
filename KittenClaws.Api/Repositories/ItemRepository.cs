
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

    private async Task<CatEntity> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<CatEntity>(id, new PartitionKey(id), null, ct);
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
        var query = _container.GetItemQueryIterator<CatEntity>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<CatEntity>();

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

    public async Task<CatDto> CreateAsync(CatEntity item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new CatDto
        {
            Id = created.Id,
            Name = created.Name,
        };
    }

    public async Task<CatDto> UpdateAsync(CatEntity item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new CatEntity
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<CatEntity>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new CatDto
        {
            Id = updated.Id,
            Name = updated.Name,
        };
    }

    public async Task<CatDto> ReplaceAsync(CatEntity item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new CatEntity
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<CatEntity>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
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
        await _container.ReplaceItemAsync<CatEntity>(item, item.Id, new PartitionKey(item.Id), null, ct);
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

    private async Task<DogEntity> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<DogEntity>(id, new PartitionKey(id), null, ct);
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
        var query = _container.GetItemQueryIterator<DogEntity>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<DogEntity>();

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

    public async Task<DogDto> CreateAsync(DogEntity item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new DogDto
        {
            Id = created.Id,
            Name = created.Name,
        };
    }

    public async Task<DogDto> UpdateAsync(DogEntity item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new DogEntity
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<DogEntity>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new DogDto
        {
            Id = updated.Id,
            Name = updated.Name,
        };
    }

    public async Task<DogDto> ReplaceAsync(DogEntity item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new DogEntity
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<DogEntity>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
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
        await _container.ReplaceItemAsync<DogEntity>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
}
