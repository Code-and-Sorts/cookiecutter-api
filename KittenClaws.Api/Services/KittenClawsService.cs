namespace KittenClaws.Api.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;

public class KittenClawsService : IKittenClawsService
{
    private readonly IKittenClawsRepository _repository;

    public KittenClawsService(IKittenClawsRepository repository)
    {
        _repository = repository;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(int limit, CancellationToken ct = default) => await _repository.GetListAsync(limit, ct);

    public async Task<KittenClawsDto> CreateAsync(CreateKittenClawsRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.CreateAsync(item.ToEntity(), userId, ct);

    public async Task<KittenClawsDto> UpdateAsync(UpdateKittenClawsRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.UpdateAsync(item.Id, item.ApplyTo, userId, ct);

    public async Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => await _repository.DeleteAsync(id, userId, ct);
}
