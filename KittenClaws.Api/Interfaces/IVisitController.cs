namespace KittenClaws.Api.Interfaces;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;

public interface IVisitController
{
    Task<VisitDto> GetAsync(string id, CancellationToken ct = default);
    Task<VisitDto> CreateAsync(Stream item, string? userId, CancellationToken ct = default);
    Task<VisitDto> UpdateAsync(string id, Stream item, string? userId, CancellationToken ct = default);
}
