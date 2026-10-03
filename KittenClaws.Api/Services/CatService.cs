namespace KittenClaws.Api.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
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
        await _repository.CreateAsync(item.ToEntity(), userId, ct);

    public async Task<CatDto> UpdateAsync(UpdateCatRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.UpdateAsync(item.Id, item.ApplyTo, userId, ct);

    public async Task<CatDto> ReplaceAsync(ReplaceCatRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.ReplaceAsync(item.Id, item.ApplyTo, userId, ct);

    public async Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => await _repository.DeleteAsync(id, userId, ct);
}
