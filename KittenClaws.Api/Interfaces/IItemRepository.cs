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

    Task<CatDto> CreateAsync(CatEntity item, CancellationToken ct = default);

    Task<CatDto> UpdateAsync(CatEntity item, CancellationToken ct = default);

    Task<CatDto> ReplaceAsync(CatEntity item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}

public interface IDogRepository
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);

    Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default);

    Task<DogDto> CreateAsync(DogEntity item, CancellationToken ct = default);

    Task<DogDto> UpdateAsync(DogEntity item, CancellationToken ct = default);

    Task<DogDto> ReplaceAsync(DogEntity item, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
