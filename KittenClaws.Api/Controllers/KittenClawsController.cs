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

public class KittenClawsController : IKittenClawsController
{
    private const string ResourceName = "KittenClaws";
    private readonly IKittenClawsService _service;

    public KittenClawsController(IKittenClawsService service)
    {
        _service = service;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        return await _service.GetAsync(id, ct);
    }

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(string? limit = null, CancellationToken ct = default) =>
        await _service.GetListAsync(Pagination.ParseLimit(limit), ct);

    public async Task<KittenClawsDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default)
    {
        var request = await RequestBody.ReadValidAsync<CreateKittenClawsRequest, CreateKittenClawsRequestValidator>(item, ct);
        return await _service.CreateAsync(request, userId, ct);
    }

    public async Task<KittenClawsDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        var request = await RequestBody.ReadValidAsync<UpdateKittenClawsRequest, UpdateKittenClawsRequestValidator>(item, ct);
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
