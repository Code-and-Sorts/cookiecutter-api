
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

public class KittenClawsRepository : IKittenClawsRepository
{
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly Container _container;

    public KittenClawsRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    private async Task<KittenClaws> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<KittenClaws>(id, new PartitionKey(id), null, ct);
        var item = response.Resource;

        if (item.IsDeleted)
        {
            throw new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0);
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
        var query = _container.GetItemQueryIterator<KittenClaws>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<KittenClaws>();

        while (query.HasMoreResults && results.Count < DefaultListLimit)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        return results.Take(DefaultListLimit).Select(item => new KittenClawsDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<KittenClawsDto> CreateAsync(KittenClaws item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new KittenClawsDto
        {
            Id = created.Id,
            Name = created.Name,
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
        var response = await _container.ReplaceItemAsync<KittenClaws>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new KittenClawsDto
        {
            Id = updated.Id,
            Name = updated.Name,
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
        var response = await _container.ReplaceItemAsync<KittenClaws>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
        var replaced = response.Resource;
        return new KittenClawsDto
        {
            Id = replaced.Id,
            Name = replaced.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;
        await _container.ReplaceItemAsync<KittenClaws>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
}
