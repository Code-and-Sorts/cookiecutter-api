namespace KittenClaws.Api.Services;

using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;

public class VisitService : IVisitService
{
    private readonly IVisitRepository _repository;

    public VisitService(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<VisitDto> GetAsync(string id, CancellationToken ct = default) => await _repository.GetAsync(id, ct);

    public async Task<VisitDto> CreateAsync(CreateVisitRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.CreateAsync(item.ToEntity(), userId, ct);

    public async Task<VisitDto> UpdateAsync(UpdateVisitRequest item, string? userId, CancellationToken ct = default) =>
        await _repository.UpdateAsync(item.Id, item.ApplyTo, userId, ct);
}
