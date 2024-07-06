using Cashlog.Application.Extensions;
using Cashlog.Data;
using Microsoft.EntityFrameworkCore;

if (args.Length > 0 && args[0] == "migrate")
{
    var migrationBuilder = WebApplication.CreateBuilder(args);
    migrationBuilder.Services
        .AddCashlogDatabase(migrationBuilder.Configuration);

    var migrationApp = migrationBuilder.Build();
    var dbProvider = migrationApp.Services.GetService<IDatabaseContextProvider>();
    await using var context = dbProvider!.Create();
    await context.Database.MigrateAsync();
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCashlog(builder.Configuration)
    .AddCashlogLogger(builder.Host)
    ;

var app = builder.Build();

if (app.Environment.IsDevelopment()
    || app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();