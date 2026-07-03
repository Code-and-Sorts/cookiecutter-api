namespace {{project_class_name}}.Api.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}Service : I{{ r }}Service
{
    private readonly I{{ r }}Repository _repository;

    public {{ r }}Service(I{{ r }}Repository repository)
    {
        _repository = repository;
    }
{%- if "get_by_id" in resource.operations %}

    public async Task<{{ r }}Dto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);
{%- endif %}
{%- if "list" in resource.operations %}

    public async Task<IEnumerable<{{ r }}Dto>> GetListAsync(CancellationToken ct = default) => await _repository.GetListAsync(ct);
{%- endif %}
{%- if "create" in resource.operations %}

    public async Task<{{ r }}Dto> CreateAsync(Create{{ r }}Request item, CancellationToken ct = default)
    {
        var new{{ r }} = new {{ r }}
        {
            Id = Guid.NewGuid().ToString(),
            Name = item.Name,
            CreatedBy = item.CreatedBy,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.CreateAsync(new{{ r }}, ct);
    }
{%- endif %}
{%- if "update" in resource.operations %}

    public async Task<{{ r }}Dto> UpdateAsync(Update{{ r }}Request item, CancellationToken ct = default)
    {
        var updated{{ r }} = new {{ r }}
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.UpdateAsync(updated{{ r }}, ct);
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    public async Task<{{ r }}Dto> ReplaceAsync(Replace{{ r }}Request item, CancellationToken ct = default)
    {
        var replaced{{ r }} = new {{ r }}
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.ReplaceAsync(replaced{{ r }}, ct);
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _repository.DeleteAsync(id, ct);
{%- endif %}
}
{% endfor %}