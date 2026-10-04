namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;

public class DynamoDocumentStore<T>(IAmazonDynamoDB client, string tableName) : IDocumentStore<T> where T : BaseEntity, new()
{
    public async Task<T?> GetAsync(string id, CancellationToken ct = default)
    {
        var response = await client.GetItemAsync(new GetItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = id } } },
        }, ct);

        return response.Item is { Count: > 0 } ? FromItem(response.Item) : null;
    }

    public async Task<IReadOnlyList<T>> GetLiveListAsync(int limit, CancellationToken ct = default)
    {
        var results = new List<T>();
        Dictionary<string, AttributeValue>? lastEvaluatedKey = null;

        // A scan's Limit counts items before the isDeleted filter, so page until enough are found.
        do
        {
            var response = await client.ScanAsync(new ScanRequest
            {
                TableName = tableName,
                FilterExpression = "isDeleted = :isDeleted",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":isDeleted", new AttributeValue { BOOL = false } } },
                ExclusiveStartKey = lastEvaluatedKey,
                Limit = limit,
            }, ct);

            results.AddRange((response.Items ?? []).Select(FromItem));
            lastEvaluatedKey = response.LastEvaluatedKey is { Count: > 0 } ? response.LastEvaluatedKey : null;
        } while (lastEvaluatedKey != null && results.Count < limit);

        return results;
    }

    public Task CreateAsync(T item, CancellationToken ct = default) => SaveAsync(item, ct);

    public async Task<T?> UpdateAsync(string id, Action<T> change, CancellationToken ct = default)
    {
        var item = await GetAsync(id, ct);
        if (item != null)
        {
            change(item);
            await SaveAsync(item, ct);
        }
        return item;
    }

    public Task SaveAsync(T item, CancellationToken ct = default) =>
        client.PutItemAsync(new PutItemRequest { TableName = tableName, Item = ToItem(item) }, ct);

    private static Dictionary<string, AttributeValue> ToItem(T entity)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue { S = entity.Id } },
            { "isDeleted", new AttributeValue { BOOL = entity.IsDeleted } },
            { "createdTimestamp", new AttributeValue { S = entity.CreatedTimestamp } },
            { "updatedTimestamp", new AttributeValue { S = entity.UpdatedTimestamp } },
        };
        if (entity.CreatedBy != null)
        {
            item["createdBy"] = new AttributeValue { S = entity.CreatedBy };
        }
        if (entity.UpdatedBy != null)
        {
            item["updatedBy"] = new AttributeValue { S = entity.UpdatedBy };
        }
        entity.WriteAttributes(item);
        return item;
    }

    private static T FromItem(Dictionary<string, AttributeValue> item)
    {
        var entity = new T
        {
            Id = item.TryGetValue("id", out var id) ? id.S : string.Empty,
            IsDeleted = item.TryGetValue("isDeleted", out var isDeleted) && isDeleted.BOOL == true,
            CreatedTimestamp = item.TryGetValue("createdTimestamp", out var createdTimestamp) ? createdTimestamp.S : string.Empty,
            UpdatedTimestamp = item.TryGetValue("updatedTimestamp", out var updatedTimestamp) ? updatedTimestamp.S : string.Empty,
            CreatedBy = item.TryGetValue("createdBy", out var createdBy) ? createdBy.S : null,
            UpdatedBy = item.TryGetValue("updatedBy", out var updatedBy) ? updatedBy.S : null,
        };
        entity.ReadAttributes(item);
        return entity;
    }
}
