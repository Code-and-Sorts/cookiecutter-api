namespace {{cookiecutter.project_name}}.Api.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_name}}.Api.Dtos;
using {{cookiecutter.project_name}}.Api.Entities;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Requests;

public class {{cookiecutter.project_name}}Service : I{{cookiecutter.project_name}}Service
{
    private readonly I{{cookiecutter.project_name}}Repository _{{cookiecutter.project_lower_camel_name}}Repository;

    public {{cookiecutter.project_name}}Service(I{{cookiecutter.project_name}}Repository {{cookiecutter.project_lower_camel_name}}Repository)
    {
        _{{cookiecutter.project_lower_camel_name}}Repository = {{cookiecutter.project_lower_camel_name}}Repository;
    }

    public async Task<{{cookiecutter.project_name}}Dto> GetAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Repository.GetAsync(id, ct);

    public async Task<IEnumerable<{{cookiecutter.project_name}}Dto>> GetListAsync(CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Repository.GetListAsync(ct);

    public async Task<{{cookiecutter.project_name}}Dto> CreateAsync(Create{{cookiecutter.project_name}}Request {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        var new{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}
        {
            Id = Guid.NewGuid().ToString(),
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
        return await _{{cookiecutter.project_lower_camel_name}}Repository.CreateAsync(new{{cookiecutter.project_name}}, ct);
    }

    public async Task<{{cookiecutter.project_name}}Dto> UpdateAsync(Update{{cookiecutter.project_name}}Request {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        var updated{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}
        {
            Id = {{cookiecutter.project_lower_camel_name}}.Id,
            Name = {{cookiecutter.project_lower_camel_name}}.Name,
        };
        return await _{{cookiecutter.project_lower_camel_name}}Repository.UpdateAsync(updated{{cookiecutter.project_name}}, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Repository.DeleteAsync(id, ct);
}
