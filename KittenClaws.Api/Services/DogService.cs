namespace KittenClaws.Api.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;

public class DogService : IDogService
{
    private readonly IDogRepository _repository;

    public DogService(IDogRepository repository)
    {
        _repository = repository;
    }

    public async Task<DogDto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);

    public async Task<IEnumerable<DogDto>> GetListAsync(int limit, CancellationToken ct = default) => await _repository.GetListAsync(limit, ct);

    public async Task<DogDto> CreateAsync(CreateDogRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.CreateAsync(item.ToEntity(), userId, ct);

    public async Task<DogDto> ReplaceAsync(ReplaceDogRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.ReplaceAsync(item.Id, item.ApplyTo, userId, ct);

    public async Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => await _repository.DeleteAsync(id, userId, ct);
}
