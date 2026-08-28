{%- if cloud_service in ['Azure Function App', 'GCP Cloud Function'] %}
namespace {{project_class_name}}.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}Repository : I{{ r }}Repository
{
{%- if cloud_service == 'Azure Function App' %}
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly Container _container;

    public {{ r }}Repository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    private async Task<{{ r }}> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<{{ r }}>(id, new PartitionKey(id), null, ct);
        var item = response.Resource;

        if (item.IsDeleted)
        {
            throw new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0);
        }
        return item;
    }

    public async Task<{{ r }}Dto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<{{ r }}Dto>> GetListAsync(CancellationToken ct = default)
    {
        var query = _container.GetItemQueryIterator<{{ r }}>($"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {DefaultListLimit}");
        var results = new List<{{ r }}>();

        while (query.HasMoreResults && results.Count < DefaultListLimit)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        return results.Take(DefaultListLimit).Select(item => new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<{{ r }}Dto> CreateAsync({{ r }} item, CancellationToken ct = default)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id), null, ct);
        var created = response.Resource;
        return new {{ r }}Dto
        {
            Id = created.Id,
            Name = created.Name,
        };
    }

    public async Task<{{ r }}Dto> UpdateAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new {{ r }}
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<{{ r }}>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var updated = response.Resource;
        return new {{ r }}Dto
        {
            Id = updated.Id,
            Name = updated.Name,
        };
    }

    public async Task<{{ r }}Dto> ReplaceAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new {{ r }}
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<{{ r }}>(replaceItem, replaceItem.Id, new PartitionKey(replaceItem.Id), null, ct);
        var replaced = response.Resource;
        return new {{ r }}Dto
        {
            Id = replaced.Id,
            Name = replaced.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;
        await _container.ReplaceItemAsync<{{ r }}>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    // Default cap on list reads to avoid unbounded queries.
    private const int DefaultListLimit = 100;
    private readonly IFirestoreContext<{{ r }}> _context;

    public {{ r }}Repository(IFirestoreContext<{{ r }}> context)
    {
        _context = context;
    }

    private async Task<{{ r }}> GetItemAsync(string id, CancellationToken ct)
    {
        var item = await _context.GetAsync(id, ct);

        if (item == null || item.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return item;
    }

    public async Task<{{ r }}Dto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<{{ r }}Dto>> GetListAsync(CancellationToken ct = default)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        return results.Take(DefaultListLimit).Select(item => new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<{{ r }}Dto> CreateAsync({{ r }} item, CancellationToken ct = default)
    {
        await _context.SetAsync(item.Id, item, ct);

        return new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<{{ r }}Dto> UpdateAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new {{ r }}
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(updateItem.Id, updateItem, ct);

        return new {{ r }}Dto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task<{{ r }}Dto> ReplaceAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new {{ r }}
        {
            Id = currentItem.Id,
            Name = item.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(replaceItem.Id, replaceItem, ct);

        return new {{ r }}Dto
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
{%- endif %}
}
{% endfor %}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
namespace {{project_class_name}}.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}Repository : I{{ r }}Repository
{
    // Default cap on list reads to avoid unbounded scans.
    private const int DefaultListLimit = 100;
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public {{ r }}Repository(IAmazonDynamoDB dynamoClient, string tableName)
    {
        _dynamoClient = dynamoClient;
        _tableName = tableName;
    }

    private static Dictionary<string, AttributeValue> ToAttributeMap({{ r }} entity)
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

    private static {{ r }} FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        return new {{ r }}
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

    private async Task<{{ r }}> GetItemAsync(string id, CancellationToken ct)
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

    public async Task<{{ r }}Dto> GetAsync(string id, CancellationToken ct = default)
    {
        var item = await GetItemAsync(id, ct);

        return new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<IEnumerable<{{ r }}Dto>> GetListAsync(CancellationToken ct = default)
    {
        var results = new List<{{ r }}>();
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

        return results.Take(DefaultListLimit).Select(item => new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        });
    }

    public async Task<{{ r }}Dto> CreateAsync({{ r }} item, CancellationToken ct = default)
    {
        await _dynamoClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = ToAttributeMap(item)
        }, ct);

        return new {{ r }}Dto
        {
            Id = item.Id,
            Name = item.Name,
        };
    }

    public async Task<{{ r }}Dto> UpdateAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new {{ r }}
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

        return new {{ r }}Dto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task<{{ r }}Dto> ReplaceAsync({{ r }} item, CancellationToken ct = default)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var replaceItem = new {{ r }}
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

        return new {{ r }}Dto
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
{% endfor %}
{%- endif %}