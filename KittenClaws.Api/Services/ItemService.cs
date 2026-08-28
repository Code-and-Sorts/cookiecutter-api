namespace KittenClaws.Api.Services;

using System;
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

    public async Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default) => await _repository.GetListAsync(ct);

    public async Task<CatDto> CreateAsync(CreateCatRequest item, CancellationToken ct = default)
    {
        var newCat = new Cat
        {
            Id = Guid.NewGuid().ToString(),
            Name = item.Name,
            CreatedBy = item.CreatedBy,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.CreateAsync(newCat, ct);
    }

    public async Task<CatDto> UpdateAsync(UpdateCatRequest item, CancellationToken ct = default)
    {
        var updatedCat = new Cat
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.UpdateAsync(updatedCat, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _repository.DeleteAsync(id, ct);
}

public class DogService : IDogService
{
    private readonly IDogRepository _repository;

    public DogService(IDogRepository repository)
    {
        _repository = repository;
    }

    public async Task<DogDto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);

    public async Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default) => await _repository.GetListAsync(ct);

    public async Task<DogDto> CreateAsync(CreateDogRequest item, CancellationToken ct = default)
    {
        var newDog = new Dog
        {
            Id = Guid.NewGuid().ToString(),
            Name = item.Name,
            CreatedBy = item.CreatedBy,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.CreateAsync(newDog, ct);
    }

    public async Task<DogDto> ReplaceAsync(ReplaceDogRequest item, CancellationToken ct = default)
    {
        var replacedDog = new Dog
        {
            Id = item.Id,
            Name = item.Name,
            UpdatedBy = item.UpdatedBy,
        };
        return await _repository.ReplaceAsync(replacedDog, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default) => await _repository.DeleteAsync(id, ct);
}
