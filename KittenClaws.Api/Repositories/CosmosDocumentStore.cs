namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using Microsoft.Azure.Cosmos;

public class CosmosDocumentStore<T>(Container container) : IDocumentStore<T> where T : BaseEntity
{
    public async Task<T?> GetAsync(string id, CancellationToken ct = default) => (await ReadAsync(id, ct))?.Resource;

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

    public async Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default)
    {
        if (await ReadAsync(id, ct) is not { } read)
        {
            return null;
        }
        var item = read.Resource;
        change(item);
        await container.ReplaceItemAsync(item, id, new PartitionKey(id), new ItemRequestOptions { IfMatchEtag = read.ETag }, ct);
        return item;
    }

    private async Task<ItemResponse<T>?> ReadAsync(string id, CancellationToken ct)
    {
        try
        {
            return await container.ReadItemAsync<T>(id, new PartitionKey(id), null, ct);
        }
        // Only substatus 0 is a missing item; other 404s (e.g. 1003, no container) are config errors.
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound && ex.SubStatusCode == 0)
        {
            return null;
        }
    }
}
