namespace {{project_class_name}}.Api.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;

public class {{project_class_name}}Service : I{{project_class_name}}Service
{
    private readonly I{{project_class_name}}Repository _{{project_lower_camel_name}}Repository;

    public {{project_class_name}}Service(I{{project_class_name}}Repository {{project_lower_camel_name}}Repository)
    {
        _{{project_lower_camel_name}}Repository = {{project_lower_camel_name}}Repository;
    }

    public async Task<{{project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default) => await _{{project_lower_camel_name}}Repository.GetAsync(id, ct);

    public async Task<IEnumerable<{{project_class_name}}Dto>> GetListAsync(CancellationToken ct = default) => await _{{project_lower_camel_name}}Repository.GetListAsync(ct);

    public async Task<{{project_class_name}}Dto> CreateAsync(Create{{project_class_name}}Request {{project_lower_camel_name}}, CancellationToken ct = default)
    {
        var new{{project_class_name}} = new {{project_class_name}}
        {
            Id = Guid.NewGuid().ToString(),
            Name = {{project_lower_camel_name}}.Name,
        };
        return await _{{project_lower_camel_name}}Repository.CreateAsync(new{{project_class_name}}, ct);
    }

    public async Task<{{project_class_name}}Dto> UpdateAsync(Update{{project_class_name}}Request {{project_lower_camel_name}}, CancellationToken ct = default)
    {
        var updated{{project_class_name}} = new {{project_class_name}}
        {
            Id = {{project_lower_camel_name}}.Id,
            Name = {{project_lower_camel_name}}.Name,
        };
        return await _{{project_lower_camel_name}}Repository.UpdateAsync(updated{{project_class_name}}, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _{{project_lower_camel_name}}Repository.DeleteAsync(id, ct);
}
