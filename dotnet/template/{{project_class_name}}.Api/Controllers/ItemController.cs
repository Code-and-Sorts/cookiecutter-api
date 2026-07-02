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

public class ItemController : IItemController
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService)
    {
        _itemService = itemService;
    }

    private static async Task<T> DeserializeRequestBodyAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        var result = JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
        return result;
    }

    public async Task<ItemDto> GetAsync(string id, CancellationToken ct = default) => await _itemService.GetAsync(id, ct);

    public async Task<IEnumerable<ItemDto>> GetListAsync(CancellationToken ct = default) => await _itemService.GetListAsync(ct);

    public async Task<ItemDto> CreateAsync(Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<CreateItemRequest>(item);
        var validator = new CreateItemRequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _itemService.CreateAsync(deserializedRequest, ct);
    }

    public async Task<ItemDto> UpdateAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<UpdateItemRequest>(item);
        deserializedRequest.Id = id;
        var validator = new UpdateItemRequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _itemService.UpdateAsync(deserializedRequest, ct);
    }

    public async Task<ItemDto> ReplaceAsync(string id, Stream item, CancellationToken ct = default)
    {
        var deserializedRequest = await DeserializeRequestBodyAsync<ReplaceItemRequest>(item);
        deserializedRequest.Id = id;
        var validator = new ReplaceItemRequestValidator();
        await validator.ValidateAndThrowAsync(deserializedRequest, ct);
        return await _itemService.ReplaceAsync(deserializedRequest, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _itemService.DeleteAsync(id, ct);
}
