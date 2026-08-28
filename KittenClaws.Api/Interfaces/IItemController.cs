namespace KittenClaws.Api.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;

public interface ICatController
{
    Task<CatDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<CatDto>> GetListAsync(CancellationToken ct = default);
    Task<CatDto> CreateAsync(Stream item, CancellationToken ct = default);
    Task<CatDto> UpdateAsync(string id, Stream item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public interface IDogController
{
    Task<DogDto> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<DogDto>> GetListAsync(CancellationToken ct = default);
    Task<DogDto> CreateAsync(Stream item, CancellationToken ct = default);
    Task<DogDto> ReplaceAsync(string id, Stream item, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
