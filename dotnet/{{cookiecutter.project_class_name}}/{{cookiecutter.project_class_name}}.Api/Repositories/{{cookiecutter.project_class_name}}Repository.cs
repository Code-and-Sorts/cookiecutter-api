namespace {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Dtos;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Entities;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Interfaces;
using Microsoft.Azure.Cosmos;

public class {{cookiecutter.{{cookiecutter.project_class_name}}}}Repository : I{{cookiecutter.{{cookiecutter.project_class_name}}}}Repository
{
    private readonly Container _container;

    public {{cookiecutter.{{cookiecutter.project_class_name}}}}Repository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto> GetAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<{{cookiecutter.{{cookiecutter.project_class_name}}}}>(id, new PartitionKey(id), null, ct);
        var {{cookiecutter.project_lower_camel_name}} = response.Resource;

        if ({{cookiecutter.project_lower_camel_name}}.IsDeleted)
        {
            throw new Exception("Item not found");
        }

        return new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
    }

    public async Task<IEnumerable<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto>> GetListAsync(CancellationToken ct)
    {
        var query = _container.GetItemQueryIterator<{{cookiecutter.{{cookiecutter.project_class_name}}}}>("SELECT * FROM c WHERE c.isDeleted = false");
        var results = new List<{{cookiecutter.{{cookiecutter.project_class_name}}}}>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(ct);
            results.AddRange(response.Resource);
        }

        var {{cookiecutter.project_lower_camel_name}}Dtos = results.Select({{cookiecutter.project_lower_camel_name}} => new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        });

        return {{cookiecutter.project_lower_camel_name}}Dtos;
    }

    public async Task<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto> CreateAsync({{cookiecutter.{{cookiecutter.project_class_name}}}} {{cookiecutter.project_lower_camel_name}}, CancellationToken ct)
    {
        var response = await _container.CreateItemAsync({{cookiecutter.project_lower_camel_name}}, new PartitionKey({{cookiecutter.project_lower_camel_name}}.Id), null, ct);
        var {{cookiecutter.project_lower_camel_name}}Dto = response.Resource;
        return new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}Dto.Id,
            Name = {{cookiecutter.project_lower_camel_name}}Dto.Name,
        };
    }

    public async Task<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto> UpdateAsync({{cookiecutter.{{cookiecutter.project_class_name}}}} item, CancellationToken ct)
    {
        var currentItem = await _container.ReadItemAsync<{{cookiecutter.{{cookiecutter.project_class_name}}}}>(item.Id, new PartitionKey(item.Id), null, ct);
        var updateItem = new {{cookiecutter.{{cookiecutter.project_class_name}}}}
        {
            Id = currentItem.Resource.Id,
            Name = currentItem.Resource.Name ?? item.Name,
            CreatedBy = currentItem.Resource.CreatedBy,
            CreatedTimestamp = currentItem.Resource.CreatedTimestamp,
            UpdatedBy = item.UpdatedBy ?? currentItem.Resource.UpdatedBy,
            UpdatedTimestamp = DateTime.UtcNow,
        };
        var response = await _container.ReplaceItemAsync<{{cookiecutter.{{cookiecutter.project_class_name}}}}>(updateItem, updateItem.Id, new PartitionKey(updateItem.Id), null, ct);
        var {{cookiecutter.project_lower_camel_name}}Response = response.Resource;
        return new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto
        {
            Id = {{cookiecutter.project_lower_camel_name}}Response.Id,
            Name = {{cookiecutter.project_lower_camel_name}}Response.Name,
        };
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        await _container.DeleteItemAsync<{{cookiecutter.{{cookiecutter.project_class_name}}}}>(id, new PartitionKey(id), null, ct);
    }
}
