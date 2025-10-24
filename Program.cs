using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.Services;
using minimal_api.Infraestructure.DB;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Entities;
using System.Text.RegularExpressions;
using minimal_api.Domain.Enums;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

// Inje��o de depend�ncia
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

// Configura��o Swagger
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
        return Results.Ok("Usu�rio logado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administrador");

app.MapPost("/administradores", ([FromBody] AdminDto adminDto, IAdminService adminService) =>
{
    // Valida��es
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDto.Email)) errors.Message.Add("O campo 'Email' � obrigat�rio.");
    if (!Regex.IsMatch(adminDto.Email, @"^[\w\.-]+@[\w-]+\.[a-zA-Z]{2,}$")) errors.Message.Add("O campo 'Email' est� em um formato inv�lido.");
    if (string.IsNullOrEmpty(adminDto.Senha)) errors.Message.Add("O campo 'Senha' � obrigat�rio.");
    if (adminDto.Perfil == null) errors.Message.Add("O campo 'Perfil' � obrigat�rio.");

    if (errors.Message.Count > 0) return Results.BadRequest(errors);

    var admin = new Administrador
    {
        Email = adminDto.Email,
        Senha = adminDto.Senha,
        Perfil = adminDto.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    adminService.Insert(admin);
    return Results.Created($"/administradores/{admin.Id}", new AdminModelView
    {
        Id = admin.Id,
        Email = admin.Email,
        Perfil = admin.Perfil
    });

}).WithTags("Administrador");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdminService adminService) =>
{
    var admins = new List<AdminModelView>();
    var adminsView = adminService.GetAll(pagina);

    foreach (var admin in adminsView)
    {
        admins.Add(new AdminModelView
        {
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil
        });
    }

    return Results.Ok(admins);
}).WithTags("Administrador");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdminService adminService) =>
{
    var admin = adminService.GetById(id);
    if (admin == null) return Results.NotFound();
    return Results.Ok(new AdminModelView
    {
        Id = admin.Id,
        Email = admin.Email,
        Perfil = admin.Perfil
    });
}).WithTags("Administrador");

app.MapPut("/administradores/{id}", ([FromRoute] int id, AdminDto adminDto, IAdminService adminService) =>
{
    var admin = adminService.GetById(id);
    if (admin == null) return Results.NotFound();

    // Valida��es
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDto.Email)) errors.Message.Add("O campo 'Email' � obrigat�rio.");
    if (!Regex.IsMatch(adminDto.Email, @"^[\w\.-]+@[\w-]+\.[a-zA-Z]{2,}$")) errors.Message.Add("O campo 'Email' est� em um formato inv�lido.");
    if (string.IsNullOrEmpty(adminDto.Senha)) errors.Message.Add("O campo 'Senha' � obrigat�rio.");
    if (adminDto.Perfil == null) errors.Message.Add("O campo 'Perfil' � obrigat�rio.");

    if (errors.Message.Count > 0) return Results.BadRequest(errors);

    admin.Email = adminDto.Email;
    admin.Senha = adminDto.Senha;
    admin.Perfil = adminDto.Perfil.ToString();
    

    adminService.Update(admin);
    return Results.Ok(new AdminModelView
    {
        Id = admin.Id,
        Email = admin.Email,
        Perfil = admin.Perfil
    });
}).WithTags("Administrador");

app.MapDelete("/administradores/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    veiculoService.Delete(veiculo);
    return Results.NoContent();
}).WithTags("Administrador");


#endregion

#region Ve�culos

// M�todo de Valida��es
ErrorView validaDTO(VeiculoDto veiculoDto)
{
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDto.Nome)) errors.Message.Add("O campo 'Nome' � obrigat�rio.");
    if (string.IsNullOrEmpty(veiculoDto.Marca)) errors.Message.Add("O campo 'Marca' � obrigat�rio.");
    if (veiculoDto.Ano < 1950) errors.Message.Add("O ve�culo � muito antigo! S� � permitido cadastrar ve�culo acima do ano de 1950.");

    return errors;
} 

app.MapPost("/veiculos", ([FromBody] VeiculoDto veiculoDto, IVeiculoService veiculoService) =>
{
    // Valida��es
    var errors = validaDTO(veiculoDto);
    if (errors.Message.Count > 0) return Results.BadRequest(errors);

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

    // Valida��es
    var errors = validaDTO(veiculoDto);
    if (errors.Message.Count > 0) return Results.BadRequest(errors);

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
// Configura��o Swagger
app.UseSwagger();
app.UseSwaggerUI();



app.Run();
#endregion