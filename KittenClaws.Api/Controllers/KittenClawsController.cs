namespace KittenClaws.Api.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Validation;
using Newtonsoft.Json;

public class KittenClawsController : IKittenClawsController
{
    private readonly IKittenClawsService _kittenClawsService;

    public KittenClawsController(IKittenClawsService kittenClawsService)
    {
        _kittenClawsService = kittenClawsService;
    }

    private static async Task<T> DeserializeRequestBodyAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        var result = JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
        return result;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => await _kittenClawsService.GetAsync(id, ct);

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default) => await _kittenClawsService.GetListAsync(ct);

    public async Task<KittenClawsDto> CreateAsync(Stream kittenClaws, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<CreateKittenClawsRequest>(kittenClaws);
        var validator = new CreateKittenClawsRequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _kittenClawsService.CreateAsync(deserializedRequest, ct);
    }

    public async Task<KittenClawsDto> UpdateAsync(string id, Stream kittenClaws, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<UpdateKittenClawsRequest>(kittenClaws);
        deserializedRequest.Id = id;
        var validator = new UpdateKittenClawsRequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _kittenClawsService.UpdateAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _kittenClawsService.DeleteAsync(id, ct);
}
