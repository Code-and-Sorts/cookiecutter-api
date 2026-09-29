namespace KittenClaws.Api.Controllers;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Validation;
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

public class CatController : ICatController
{
    private readonly ICatService _service;

    public CatController(ICatService service)
    {
        _service = service;
    }

    public async Task<CatDto> GetAsync(string id, CancellationToken ct = default) => await _service.GetAsync(id, ct);

    public async Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default) => await _service.GetListAsync(ct);

    public async Task<CatDto> CreateAsync(Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<CreateCatRequest>(item);
        await new CreateCatRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.CreateAsync(deserializedRequest, ct);
    }

    public async Task<CatDto> UpdateAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<UpdateCatRequest>(item);
        deserializedRequest.Id = id;
        await new UpdateCatRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.UpdateAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _service.DeleteAsync(id, ct);
}

public class DogController : IDogController
{
    private readonly IDogService _service;

    public DogController(IDogService service)
    {
        _service = service;
    }

    public async Task<DogDto> GetAsync(string id, CancellationToken ct = default) => await _service.GetAsync(id, ct);

    public async Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default) => await _service.GetListAsync(ct);

    public async Task<DogDto> CreateAsync(Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<CreateDogRequest>(item);
        await new CreateDogRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.CreateAsync(deserializedRequest, ct);
    }

    public async Task<DogDto> ReplaceAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<ReplaceDogRequest>(item);
        deserializedRequest.Id = id;
        await new ReplaceDogRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.ReplaceAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _service.DeleteAsync(id, ct);
}
