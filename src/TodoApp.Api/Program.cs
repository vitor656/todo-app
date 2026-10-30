using TodoApp.Api;
using TodoApp.Application;
using TodoApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

var app = builder.Build();

await app.MigrateDatabaseAsync();
app.UseApi();
app.MapDefaultEndpoints();

app.Run();
