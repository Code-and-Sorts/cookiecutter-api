namespace KittenClaws.Api.Interfaces;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface IResourceHandler
{
    string Endpoint { get; }

    Task HandleAsync(HttpContext context, string? id);
}
