namespace KittenClaws.Api.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
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

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default) => await _repository.GetListAsync(ct);

    public async Task<KittenClawsDto> CreateAsync(CreateKittenClawsRequest item, CancellationToken ct = default)
    {
        var newKittenClaws = new KittenClaws
        {
            Id = Guid.NewGuid().ToString(),
            Name = item.Name,
            CreatedBy = item.CreatedBy,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.CreateAsync(newKittenClaws, ct);
    }

    public async Task<KittenClawsDto> UpdateAsync(UpdateKittenClawsRequest item, CancellationToken ct = default)
    {
        var updatedKittenClaws = new KittenClaws
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.UpdateAsync(updatedKittenClaws, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _repository.DeleteAsync(id, ct);
}
