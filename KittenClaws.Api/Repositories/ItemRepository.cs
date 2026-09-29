
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

public class CatRepository : ICatRepository
{
    // Default cap on list reads to avoid unbounded scans.
    private const int DefaultListLimit = 100;
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public CatRepository(IAmazonDynamoDB dynamoClient, string tableName)
    {
        _dynamoClient = dynamoClient;
        _tableName = tableName;
    }

    private static Dictionary<string, AttributeValue> ToAttributeMap(CatEntity entity)
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

    private static CatEntity FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        return new CatEntity
        {
            Id = item.TryGetValue("id", out var id) ? id.S : string.Empty,
            Name = item.TryGetValue("name", out var name) ? name.S : string.Empty,
            IsDeleted = item.TryGetValue("isDeleted", out var isDeleted) && isDeleted.BOOL == true,
            CreatedTimestamp = item.TryGetValue("createdTimestamp", out var ct) ? DateTime.Parse(ct.S) : DateTime.UtcNow,
            UpdatedTimestamp = item.TryGetValue("updatedTimestamp", out var ut) ? DateTime.Parse(ut.S) : DateTime.UtcNow,
            CreatedBy = item.TryGetValue("createdBy", out var cb) ? cb.S : string.Empty,
            UpdatedBy = item.TryGetValue("updatedBy", out var ub) ? ub.S : string.Empty,
        };
    }

    private async Task<CatEntity> GetItemAsync(string id, CancellationToken ct)
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
        var results = new List<CatEntity>();
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
            results.AddRange((response.Items ?? []).Select(FromAttributeMap));
            lastEvaluatedKey = response.LastEvaluatedKey is { Count: > 0 } ? response.LastEvaluatedKey : null;
        } while (lastEvaluatedKey != null && results.Count < DefaultListLimit);

        return results.Take(DefaultListLimit).Select(item => new CatDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<CatDto> CreateAsync(CatEntity item, CancellationToken ct = default)
    {
        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);

        return new CatDto
        {
            Id = item.Id,
            Name = item.Name,
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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(updateItem)
        }, ct);

        return new CatDto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(replaceItem)
        }, ct);

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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);
    }
}

public class DogRepository : IDogRepository
{
    // Default cap on list reads to avoid unbounded scans.
    private const int DefaultListLimit = 100;
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public DogRepository(IAmazonDynamoDB dynamoClient, string tableName)
    {
        _dynamoClient = dynamoClient;
        _tableName = tableName;
    }

    private static Dictionary<string, AttributeValue> ToAttributeMap(DogEntity entity)
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

    private static DogEntity FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        return new DogEntity
        {
            Id = item.TryGetValue("id", out var id) ? id.S : string.Empty,
            Name = item.TryGetValue("name", out var name) ? name.S : string.Empty,
            IsDeleted = item.TryGetValue("isDeleted", out var isDeleted) && isDeleted.BOOL == true,
            CreatedTimestamp = item.TryGetValue("createdTimestamp", out var ct) ? DateTime.Parse(ct.S) : DateTime.UtcNow,
            UpdatedTimestamp = item.TryGetValue("updatedTimestamp", out var ut) ? DateTime.Parse(ut.S) : DateTime.UtcNow,
            CreatedBy = item.TryGetValue("createdBy", out var cb) ? cb.S : string.Empty,
            UpdatedBy = item.TryGetValue("updatedBy", out var ub) ? ub.S : string.Empty,
        };
    }

    private async Task<DogEntity> GetItemAsync(string id, CancellationToken ct)
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
        var results = new List<DogEntity>();
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
            results.AddRange((response.Items ?? []).Select(FromAttributeMap));
            lastEvaluatedKey = response.LastEvaluatedKey is { Count: > 0 } ? response.LastEvaluatedKey : null;
        } while (lastEvaluatedKey != null && results.Count < DefaultListLimit);

        return results.Take(DefaultListLimit).Select(item => new DogDto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<DogDto> CreateAsync(DogEntity item, CancellationToken ct = default)
    {
        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);

        return new DogDto
        {
            Id = item.Id,
            Name = item.Name,
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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(updateItem)
        }, ct);

        return new DogDto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(replaceItem)
        }, ct);

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

        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);
    }
}
