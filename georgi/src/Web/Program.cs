using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetRequiredSection("DefaultConnection").Get<string>();

builder.Services.AddScoped<DbContextOptions<WriteDbContext>>(_ =>
    new DbContextOptionsBuilder<WriteDbContext>().UseSqlServer(connectionString).Options);

builder.Services.AddScoped<IWriteDbContext, WriteDbContext>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
