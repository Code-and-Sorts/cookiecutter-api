namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Requests;

public interface ICatService
{
    Task<CatDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default);
    Task<CatDto> CreateAsync(CreateCatRequest item, CancellationToken ct = default);
    Task<CatDto> UpdateAsync(UpdateCatRequest item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public interface IDogService
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default);
    Task<DogDto> CreateAsync(CreateDogRequest item, CancellationToken ct = default);
    Task<DogDto> ReplaceAsync(ReplaceDogRequest item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
