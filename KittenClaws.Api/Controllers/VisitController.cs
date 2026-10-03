namespace KittenClaws.Api.Controllers;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using KittenClaws.Api.Validation;

public class VisitController : IVisitController
{
    private const string ResourceName = "Visit";
    private readonly IVisitService _service;

    public VisitController(IVisitService service)
    {
        _service = service;
    }

    public async Task<VisitDto> GetAsync(string id, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        return await _service.GetAsync(id, ct);
    }

    public async Task<VisitDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default)
    {
        var request = await RequestBody.ReadValidAsync<CreateVisitRequest, CreateVisitRequestValidator>(item, ct);
        return await _service.CreateAsync(request, userId, ct);
    }

    public async Task<VisitDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default)
    {
        ItemIds.EnsureValid(ResourceName, id);
        var request = await RequestBody.ReadValidAsync<UpdateVisitRequest, UpdateVisitRequestValidator>(item, ct);
        request.Id = id;
        return await _service.UpdateAsync(request, userId, ct);
    }
}
