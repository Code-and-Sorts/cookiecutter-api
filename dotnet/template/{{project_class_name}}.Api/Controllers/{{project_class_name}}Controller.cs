namespace {{project_class_name}}.Api.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Validation;
using Newtonsoft.Json;

public class {{project_class_name}}Controller : I{{project_class_name}}Controller
{
    private readonly I{{project_class_name}}Service _{{project_lower_camel_name}}Service;

    public {{project_class_name}}Controller(I{{project_class_name}}Service {{project_lower_camel_name}}Service)
    {
        _{{project_lower_camel_name}}Service = {{project_lower_camel_name}}Service;
    }

    private static async Task<T> DeserializeRequestBodyAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        var result = JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
        return result;
    }

    public async Task<{{project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default) => await _{{project_lower_camel_name}}Service.GetAsync(id, ct);

    public async Task<IEnumerable<{{project_class_name}}Dto>> GetListAsync(CancellationToken ct = default) => await _{{project_lower_camel_name}}Service.GetListAsync(ct);

    public async Task<{{project_class_name}}Dto> CreateAsync(Stream {{project_lower_camel_name}}, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<Create{{project_class_name}}Request>({{project_lower_camel_name}});
        var validator = new Create{{project_class_name}}RequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _{{project_lower_camel_name}}Service.CreateAsync(deserializedRequest, ct);
    }

    public async Task<{{project_class_name}}Dto> UpdateAsync(string id, Stream {{project_lower_camel_name}}, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<Update{{project_class_name}}Request>({{project_lower_camel_name}});
        deserializedRequest.Id = id;
        var validator = new Update{{project_class_name}}RequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _{{project_lower_camel_name}}Service.UpdateAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _{{project_lower_camel_name}}Service.DeleteAsync(id, ct);
}
