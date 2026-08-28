
namespace KittenClaws.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

public class KittenClawsRepository : IKittenClawsRepository
{
    // Default cap on list reads to avoid unbounded scans.
    private const int DefaultListLimit = 100;
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public KittenClawsRepository(IAmazonDynamoDB dynamoClient, string tableName)
    {
        _dynamoClient = dynamoClient;
        _tableName = tableName;
    }

    private static Dictionary<string, AttributeValue> ToAttributeMap(KittenClaws entity)
    {
        return new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue { S = entity.Id } },
            { "name", new AttributeValue { S = entity.Name } },
            { "isDeleted", new AttributeValue { BOOL = entity.IsDeleted } },
            { "createdTimestamp", new AttributeValue { S = entity.CreatedTimestamp.ToString("o") } },
            { "updatedTimestamp", new AttributeValue { S = entity.UpdatedTimestamp.ToString("o") } },
            { "createdBy", new AttributeValue { S = entity.CreatedBy ?? string.Empty } },
            { "updatedBy", new AttributeValue { S = entity.UpdatedBy ?? string.Empty } },
        };
    }

    private static KittenClaws FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        return new KittenClaws
        {
            Id = item.TryGetValue("id", out var id) ? id.S : string.Empty,
            Name = item.TryGetValue("name", out var name) ? name.S : string.Empty,
            IsDeleted = item.TryGetValue("isDeleted", out var isDeleted) && isDeleted.BOOL,
            CreatedTimestamp = item.TryGetValue("createdTimestamp", out var ct) ? DateTime.Parse(ct.S) : DateTime.UtcNow,
            UpdatedTimestamp = item.TryGetValue("updatedTimestamp", out var ut) ? DateTime.Parse(ut.S) : DateTime.UtcNow,
            CreatedBy = item.TryGetValue("createdBy", out var cb) ? cb.S : string.Empty,
            UpdatedBy = item.TryGetValue("updatedBy", out var ub) ? ub.S : string.Empty,
        };
    }

    private async Task<KittenClaws> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _dynamoClient.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } }
            }
        }, ct);

        if (response.Item == null || response.Item.Count == 0)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        var entity = FromAttributeMap(response.Item);

        if (entity.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return entity;
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
        var results = new List<KittenClaws>();
        Dictionary<string, AttributeValue>? lastEvaluatedKey = null;

        do
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                FilterExpression = "isDeleted = :isDeleted",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":isDeleted", new AttributeValue { BOOL = false } }
                },
                ExclusiveStartKey = lastEvaluatedKey,
                Limit = DefaultListLimit
            };

            var response = await _dynamoClient.ScanAsync(request, ct);
            results.AddRange(response.Items.Select(FromAttributeMap));
            lastEvaluatedKey = response.LastEvaluatedKey.Count > 0 ? response.LastEvaluatedKey : null;
        } while (lastEvaluatedKey != null && results.Count < DefaultListLimit);

        return results.Take(DefaultListLimit).Select(item => new KittenClawsDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<KittenClawsDto> CreateAsync(KittenClaws item, CancellationToken ct = default)
    {
        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);

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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(updateItem)
        }, ct);

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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(replaceItem)
        }, ct);

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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);
    }
}
