namespace KittenClaws.Api.Repositories;

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
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
        // Only substatus 0 is a missing item; other 404s (e.g. 1003, no container) are config errors.
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound && ex.SubStatusCode == 0)
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

    public Task SaveAsync(T item, CancellationToken ct = default) =>
        container.ReplaceItemAsync(item, item.Id, new PartitionKey(item.Id), null, ct);
}
