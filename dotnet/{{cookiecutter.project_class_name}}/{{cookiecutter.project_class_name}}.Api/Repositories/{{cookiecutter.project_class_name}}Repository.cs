namespace {{cookiecutter.project_class_name}}.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Entities;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
{%- endif %}

public class {{cookiecutter.project_class_name}}Repository : I{{cookiecutter.project_class_name}}Repository
{
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
    private readonly Container _container;

    public {{cookiecutter.project_class_name}}Repository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    private async Task<{{cookiecutter.project_class_name}}> GetItemAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<{{cookiecutter.project_class_name}}>(id, new PartitionKey(id), null, ct);
        var {{cookiecutter.project_lower_camel_name}} = response.Resource;

        if ({{cookiecutter.project_lower_camel_name}}.IsDeleted)
        {
            throw new CosmosException("Item not found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0);
        }
        return {{cookiecutter.project_lower_camel_name}};
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct)
    {
        var {{cookiecutter.project_lower_camel_name}} = await GetItemAsync(id, ct);

        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
    }

    public async Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct)
    {
        var query = _container.GetItemQueryIterator<{{cookiecutter.project_class_name}}>("SELECT * FROM c WHERE c.isDeleted = false");
        var results = new List<{{cookiecutter.project_class_name}}>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        var {{cookiecutter.project_lower_camel_name}}Dtos = results.Select({{cookiecutter.project_lower_camel_name}} => new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        });

        return {{cookiecutter.project_lower_camel_name}}Dtos;
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> CreateAsync({{cookiecutter.project_class_name}} {{cookiecutter.project_lower_camel_name}}, CancellationToken ct)
    {
        var response = await _container.CreateItemAsync({{cookiecutter.project_lower_camel_name}}, new PartitionKey({{cookiecutter.project_lower_camel_name}}.Id), null, ct);
        var {{cookiecutter.project_lower_camel_name}}Dto = response.Resource;
        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}Dto.Id,
            Name = {{cookiecutter.project_lower_camel_name}}Dto.Name,
        };
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync({{cookiecutter.project_class_name}} item, CancellationToken ct)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new {{cookiecutter.project_class_name}}
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<{{cookiecutter.project_class_name}}>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var {{cookiecutter.project_lower_camel_name}}Response = response.Resource;
        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}Response.Id,
            Name = {{cookiecutter.project_lower_camel_name}}Response.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;
        var response = await _container.ReplaceItemAsync<{{cookiecutter.project_class_name}}>(item, item.Id, new PartitionKey(item.Id), null, ct);
    }
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    private readonly IFirestoreContext<{{cookiecutter.project_class_name}}> _context;

    public {{cookiecutter.project_class_name}}Repository(IFirestoreContext<{{cookiecutter.project_class_name}}> context)
    {
        _context = context;
    }

    private async Task<{{cookiecutter.project_class_name}}> GetItemAsync(string id, CancellationToken ct)
    {
        var {{cookiecutter.project_lower_camel_name}} = await _context.GetAsync(id, ct);

        if ({{cookiecutter.project_lower_camel_name}} == null || {{cookiecutter.project_lower_camel_name}}.IsDeleted)
        {
            throw new KeyNotFoundException($"Item with id {id} not found.");
        }

        return {{cookiecutter.project_lower_camel_name}};
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct)
    {
        var {{cookiecutter.project_lower_camel_name}} = await GetItemAsync(id, ct);

        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
    }

    public async Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct)
    {
        var results = await _context.GetListAsync("isDeleted", false, ct);

        var {{cookiecutter.project_lower_camel_name}}Dtos = results.Select({{cookiecutter.project_lower_camel_name}} => new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        });

        return {{cookiecutter.project_lower_camel_name}}Dtos;
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> CreateAsync({{cookiecutter.project_class_name}} {{cookiecutter.project_lower_camel_name}}, CancellationToken ct)
    {
        await _context.SetAsync({{cookiecutter.project_lower_camel_name}}.Id, {{cookiecutter.project_lower_camel_name}}, ct);

        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync({{cookiecutter.project_class_name}} item, CancellationToken ct)
    {
        var currentItem = await GetItemAsync(item.Id, ct);
        var updateItem = new {{cookiecutter.project_class_name}}
        {
            Id = currentItem.Id,
            Name = item.Name ?? currentItem.Name,
            CreatedBy = currentItem.CreatedBy,
            CreatedTimestamp = currentItem.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };

        await _context.SetAsync(updateItem.Id, updateItem, ct);

        return new {{cookiecutter.project_class_name}}Dto
        {
            Id = updateItem.Id,
            Name = updateItem.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        var item = await GetItemAsync(id, ct);
        item.IsDeleted = true;

        await _context.SetAsync(item.Id, item, ct);
    }
{%- endif %}
}
