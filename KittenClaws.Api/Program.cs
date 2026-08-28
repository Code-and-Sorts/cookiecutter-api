
using KittenClaws.Api;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Build().Run();
