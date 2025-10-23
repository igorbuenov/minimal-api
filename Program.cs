using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Infraestructure.DB;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Entities;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

// Injeção de dependência
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

// Configuração Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Minimal API - Veiculos", 
        Version = "v1" 
    });
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new HomeView())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDto loginDto, IAdminService adminService) =>
{
    if (adminService.Login(loginDto) != null)
    {
        return Results.Ok("Usuário logado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administrador");
#endregion

#region Veículos

app.MapPost("/veiculos", ([FromBody] VeiculoDto veiculoDto, IVeiculoService veiculoService) =>
{

    // Validações
    var errors = new ErrorView();

    if(string.IsNullOrEmpty(veiculoDto.Nome)) errors.Message.Add("O campo 'Nome' é obrigatório.");
    if(string.IsNullOrEmpty(veiculoDto.Marca)) errors.Message.Add("O campo 'Marca' é obrigatório.");
    if(veiculoDto.Ano < 1950) errors.Message.Add("O veículo é muito antigo! Só é permitido cadastrar veículo acima do ano de 1950.");

    if(errors.Message.Count > 0) return Results.BadRequest(errors);

    var veiculo = new Veiculo
    {
        Nome = veiculoDto.Nome,
        Marca = veiculoDto.Marca,
        Ano = veiculoDto.Ano
    };

    veiculoService.Insert(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.GetAll(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDto veiculoDto, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = veiculoDto.Nome;
    veiculo.Marca = veiculoDto.Marca;
    veiculo.Ano = veiculoDto.Ano;

    veiculoService.Update(veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    veiculoService.Delete(veiculo);
    return Results.NoContent();
}).WithTags("Veiculos");

#endregion

#region App
// Configuração Swagger
app.UseSwagger();
app.UseSwaggerUI();



app.Run();
#endregion