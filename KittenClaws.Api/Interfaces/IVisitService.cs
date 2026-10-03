namespace KittenClaws.Api.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Requests;

public interface IVisitService
{
    Task<VisitDto> GetAsync(string id, CancellationToken ct = default);
    Task<VisitDto> CreateAsync(CreateVisitRequest item, string? userId, CancellationToken ct = default);
    Task<VisitDto> UpdateAsync(UpdateVisitRequest item, string? userId, CancellationToken ct = default);
}
