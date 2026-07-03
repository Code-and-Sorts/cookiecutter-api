namespace {{project_class_name}}.Api.Controllers;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Validation;
using Newtonsoft.Json;

file static class RequestBody
{
    public static async Task<T> DeserializeAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
    }
}
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}Controller : I{{ r }}Controller
{
    private readonly I{{ r }}Service _service;

    public {{ r }}Controller(I{{ r }}Service service)
    {
        _service = service;
    }
{%- if "get_by_id" in resource.operations %}

    public async Task<{{ r }}Dto> GetAsync(string id, CancellationToken ct = default) => await _service.GetAsync(id, ct);
{%- endif %}
{%- if "list" in resource.operations %}

    public async Task<IEnumerable<{{ r }}Dto>> GetListAsync(CancellationToken ct = default) => await _service.GetListAsync(ct);
{%- endif %}
{%- if "create" in resource.operations %}

    public async Task<{{ r }}Dto> CreateAsync(Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<Create{{ r }}Request>(item);
        await new Create{{ r }}RequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.CreateAsync(deserializedRequest, ct);
    }
{%- endif %}
{%- if "update" in resource.operations %}

    public async Task<{{ r }}Dto> UpdateAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<Update{{ r }}Request>(item);
        deserializedRequest.Id = id;
        await new Update{{ r }}RequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.UpdateAsync(deserializedRequest, ct);
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    public async Task<{{ r }}Dto> ReplaceAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<Replace{{ r }}Request>(item);
        deserializedRequest.Id = id;
        await new Replace{{ r }}RequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.ReplaceAsync(deserializedRequest, ct);
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _service.DeleteAsync(id, ct);
{%- endif %}
}
{% endfor %}