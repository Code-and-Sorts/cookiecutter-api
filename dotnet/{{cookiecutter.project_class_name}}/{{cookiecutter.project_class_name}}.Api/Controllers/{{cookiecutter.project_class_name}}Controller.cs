namespace {{cookiecutter.project_class_name}}.Api.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using {{cookiecutter.project_class_name}}.Api.Dtos;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Requests;
using {{cookiecutter.project_class_name}}.Api.Validation;
using Newtonsoft.Json;

public class {{cookiecutter.project_class_name}}Controller : I{{cookiecutter.project_class_name}}Controller
{
    private readonly I{{cookiecutter.project_class_name}}Service _{{cookiecutter.project_lower_camel_name}}Service;

    public {{cookiecutter.project_class_name}}Controller(I{{cookiecutter.project_class_name}}Service {{cookiecutter.project_lower_camel_name}}Service)
    {
        _{{cookiecutter.project_lower_camel_name}}Service = {{cookiecutter.project_lower_camel_name}}Service;
    }

    private static async Task<T> DeserializeRequestBodyAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        var result = JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
        return result;
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> GetAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.GetAsync(id, ct);

    public async Task<IEnumerable<{{cookiecutter.project_class_name}}Dto>> GetListAsync(CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.GetListAsync(ct);

    public async Task<{{cookiecutter.project_class_name}}Dto> CreateAsync(Stream {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<Create{{cookiecutter.project_class_name}}Request>({{cookiecutter.project_lower_camel_name}});
        var validator = new Create{{cookiecutter.project_class_name}}RequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _{{cookiecutter.project_lower_camel_name}}Service.CreateAsync(deserializedRequest, ct);
    }

    public async Task<{{cookiecutter.project_class_name}}Dto> UpdateAsync(string id, Stream {{cookiecutter.project_lower_camel_name}}, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<Update{{cookiecutter.project_class_name}}Request>({{cookiecutter.project_lower_camel_name}});
        deserializedRequest.Id = id;
        var validator = new Update{{cookiecutter.project_class_name}}RequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _{{cookiecutter.project_lower_camel_name}}Service.UpdateAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _{{cookiecutter.project_lower_camel_name}}Service.DeleteAsync(id, ct);
}
