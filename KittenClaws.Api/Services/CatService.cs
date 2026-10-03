namespace KittenClaws.Api.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;

public class CatService : ICatService
{
    private readonly ICatRepository _repository;

    public CatService(ICatRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatDto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);

    public async Task<IEnumerable<CatDto>> GetListAsync(int limit, CancellationToken ct = default) => await _repository.GetListAsync(limit, ct);

    public async Task<CatDto> CreateAsync(CreateCatRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.CreateAsync(new CatEntity { Name = item.Name }, userId, ct);

    public async Task<CatDto> UpdateAsync(UpdateCatRequest item, string? userId, CancellationToken ct = default)
    {
        var updatedCat = new CatEntity
        {
            Id = item.Id,
            // A null name (not sent in the PATCH body) keeps the stored name.
            Name = item.Name!,
        };
        return await _repository.UpdateAsync(updatedCat, userId, ct);
    }

    public async Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => await _repository.DeleteAsync(id, userId, ct);
}
