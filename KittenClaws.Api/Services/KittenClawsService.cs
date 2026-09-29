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
    private readonly IKittenClawsRepository _kittenClawsRepository;

    public KittenClawsService(IKittenClawsRepository kittenClawsRepository)
    {
        _kittenClawsRepository = kittenClawsRepository;
    }

    public async Task<KittenClawsDto> GetAsync(string id, CancellationToken ct = default) => await _kittenClawsRepository.GetAsync(id, ct);

    public async Task<IEnumerable<KittenClawsDto>> GetListAsync(CancellationToken ct = default) => await _kittenClawsRepository.GetListAsync(ct);

    public async Task<KittenClawsDto> CreateAsync(CreateKittenClawsRequest kittenClaws, CancellationToken ct = default)
    {
        var newKittenClaws = new KittenClaws
        {
            Id = Guid.NewGuid().ToString(),
            Name = kittenClaws.Name,
        };
        return await _kittenClawsRepository.CreateAsync(newKittenClaws, ct);
    }

    public async Task<KittenClawsDto> UpdateAsync(UpdateKittenClawsRequest kittenClaws, CancellationToken ct = default)
    {
        var updatedKittenClaws = new KittenClaws
        {
            Id = kittenClaws.Id,
            Name = kittenClaws.Name,
        };
        return await _kittenClawsRepository.UpdateAsync(updatedKittenClaws, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _kittenClawsRepository.DeleteAsync(id, ct);
}
