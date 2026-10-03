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

public class CatController : ICatController
{
    private const string ResourceName = "Cat";
    private readonly ICatService _service;

    public CatController(ICatService service)
    {
        _service = service;
    }

    public async Task<CatDto> GetAsync(string id, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        return await _service.GetAsync(id, ct);
    }

    public async Task<IEnumerable<CatDto>> GetListAsync(string? limit = null, CancellationToken ct = default) =>
        await _service.GetListAsync(Pagination.ParseLimit(limit), ct);

    public async Task<CatDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default)
    {
        var request = await RequestBody.ReadValidAsync<CreateCatRequest, CreateCatRequestValidator>(item, ct);
        return await _service.CreateAsync(request, userId, ct);
    }

    public async Task<CatDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        var request = await RequestBody.ReadValidAsync<UpdateCatRequest, UpdateCatRequestValidator>(item, ct);
        request.Id = id;
        return await _service.UpdateAsync(request, userId, ct);
    }

    public async Task<DeleteOkObjectResult> DeleteAsync(string id, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        await _service.DeleteAsync(id, userId, ct);
        return DeleteOkObjectResult.For(ResourceName, id);
    }
}
