namespace KittenClaws.Api.Controllers;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using KittenClaws.Api.Validation;

public class DogController : IDogController
{
    private const string ResourceName = "Dog";
    private readonly IDogService _service;

    public DogController(IDogService service)
    {
        _service = service;
    }

    public async Task<DogDto> GetAsync(string id, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        return await _service.GetAsync(id, ct);
    }

    public async Task<IEnumerable<DogDto>> GetListAsync(string? limit = null, CancellationToken ct = default) =>
        await _service.GetListAsync(Pagination.ParseLimit(limit), ct);

    public async Task<DogDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default)
    {
        var request = await RequestBody.ReadValidAsync<CreateDogRequest, CreateDogRequestValidator>(item, ct);
        return await _service.CreateAsync(request, userId, ct);
    }

    public async Task<DogDto> ReplaceAsync(string id, Stream item, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        var request = await RequestBody.ReadValidAsync<ReplaceDogRequest, ReplaceDogRequestValidator>(item, ct);
        request.Id = id;
        return await _service.ReplaceAsync(request, userId, ct);
    }

    public async Task<DeleteOkObjectResult> DeleteAsync(string id, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        await _service.DeleteAsync(id, userId, ct);
        return DeleteOkObjectResult.For(ResourceName, id);
    }
}
