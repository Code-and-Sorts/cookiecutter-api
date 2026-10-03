namespace KittenClaws.Api.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
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
        await _repository.CreateAsync(new DogEntity { Name = item.Name }, userId, ct);

    public async Task<DogDto> ReplaceAsync(ReplaceDogRequest item, string? userId, CancellationToken ct = default)
    {
        var replacedDog = new DogEntity
        {
            Id = item.Id,
            Name = item.Name,
        };
        return await _repository.ReplaceAsync(replacedDog, userId, ct);
    }

    public async Task DeleteAsync(string id, string? userId, CancellationToken ct = default) => await _repository.DeleteAsync(id, userId, ct);
}
