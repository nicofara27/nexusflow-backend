using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
