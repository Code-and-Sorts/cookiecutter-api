namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;
using Microsoft.Azure.Cosmos;

public class CosmosDocumentStore<T>(Container container) : IDocumentStore<T> where T : BaseEntity
{
    public async Task<T?> GetAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(id), null, ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (IsMissingItem(ex))
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default)
    {
        // limit is an int parsed and clamped by the controller, so it is safe to inline.
        using var query = container.GetItemQueryIterator<T>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {limit}");
        var results = new List<T>();

        while (query.HasMoreResults && results.Count < limit)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        return results;
    }

    public Task CreateAsync(T item, CancellationToken ct = default) =>
        container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);

    // The document read is written back, keeping fields other resources in the container store, and only if its
    // ETag still matches, so a write that races another fails instead of losing it.
    public async Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default)
    {
        var partition = new PartitionKey(id);
        ItemResponse<JsonObject> read;
        try
        {
            read = await container.ReadItemAsync<JsonObject>(id, partition, null, ct);
        }
        catch (CosmosException ex) when (IsMissingItem(ex))
        {
            return null;
        }
        var stored = read.Resource;
        var item = stored.Deserialize<T>(Json.Options)!;
        change(item);
        foreach (var (name, value) in Json.StoredFields(item))
        {
            if (value is { } set)
            {
                stored[name] = JsonSerializer.SerializeToNode(set);
            }
            else
            {
                stored.Remove(name);
            }
        }
        await container.ReplaceItemAsync(stored, id, partition, new ItemRequestOptions { IfMatchEtag = read.ETag }, ct);
        return item;
    }

    // Only substatus 0 is a missing item; other 404s (e.g. 1003, no container) are config errors.
    private static bool IsMissingItem(CosmosException ex) => ex.StatusCode == HttpStatusCode.NotFound && ex.SubStatusCode == 0;
}
