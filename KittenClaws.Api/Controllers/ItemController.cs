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

public class KittenClawsController : IKittenClawsController
{
    private readonly IKittenClawsService _service;

    public KittenClawsController(IKittenClawsService service)
    {
        _service = service;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => await _service.GetAsync(id, ct);

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default) => await _service.GetListAsync(ct);

    public async Task<KittenClawsDto> CreateAsync(Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<CreateKittenClawsRequest>(item);
        await new CreateKittenClawsRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.CreateAsync(deserializedRequest, ct);
    }

    public async Task<KittenClawsDto> UpdateAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await RequestBody.DeserializeAsync<UpdateKittenClawsRequest>(item);
        deserializedRequest.Id = id;
        await new UpdateKittenClawsRequestValidator().ValidateAndThrowAsync(deserializedRequest, ct);
        return await _service.UpdateAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _service.DeleteAsync(id, ct);
}
