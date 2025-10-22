using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Infraestructure.DB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddScoped<IAdminService, AdminService>();



var app = builder.Build();

app.MapGet("/", () => "Ola Mundo!");


app.MapPost("/login", ([FromBody] LoginDto loginDto, IAdminService adminService) =>
{
    if (adminService.Login(loginDto) != null)
    {
        return Results.Ok("Usuário logado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
});






app.Run();
