namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;

public interface ICatRepository
{
    Task<CatDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default);

    Task<CatDto> CreateAsync(Cat item, CancellationToken ct = default);

    Task<CatDto> UpdateAsync(Cat item, CancellationToken ct = default);

    Task<CatDto> ReplaceAsync(Cat item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}

public interface IDogRepository
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default);

    Task<DogDto> CreateAsync(Dog item, CancellationToken ct = default);

    Task<DogDto> UpdateAsync(Dog item, CancellationToken ct = default);

    Task<DogDto> ReplaceAsync(Dog item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
