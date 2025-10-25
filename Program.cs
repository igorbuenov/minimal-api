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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

#region Builder
var builder = WebApplication.CreateBuilder(args);

// Configuração Jwt Bearer
var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication( option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

// Configuração do DbContext
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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "Jwt",
        In = ParameterLocation.Header,
        Description = "Insira o token Jwt"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new HomeView())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores

string GetToken(Administrador admin)
{
    if(string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", admin.Email),
        new Claim("Perfil", admin.Perfil),
        new Claim(ClaimTypes.Role, admin.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
} 

app.MapPost("/administradores/login", ([FromBody] LoginDto loginDto, IAdminService adminService) =>
{

    var admin = adminService.Login(loginDto);

    if (admin != null)
    {
        string token = GetToken(admin);
        return Results.Ok(new AdminLoggedIn
        {
            Email = admin.Email,
            Perfil = admin.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administrador");

app.MapPost("/administradores", ([FromBody] AdminDto adminDto, IAdminService adminService) =>
{
    // Validações
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDto.Email)) errors.Message.Add("O campo 'Email' é obrigatório.");
    if (!Regex.IsMatch(adminDto.Email, @"^[\w\.-]+@[\w-]+\.[a-zA-Z]{2,}$")) errors.Message.Add("O campo 'Email' está em um formato inválido.");
    if (string.IsNullOrEmpty(adminDto.Senha)) errors.Message.Add("O campo 'Senha' é obrigatório.");
    if (adminDto.Perfil == null) errors.Message.Add("O campo 'Perfil' é obrigatório.");

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

}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrador");

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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrador");

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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrador");

app.MapPut("/administradores/{id}", ([FromRoute] int id, AdminDto adminDto, IAdminService adminService) =>
{
    var admin = adminService.GetById(id);
    if (admin == null) return Results.NotFound();

    // Validações
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDto.Email)) errors.Message.Add("O campo 'Email' é obrigatório.");
    if (!Regex.IsMatch(adminDto.Email, @"^[\w\.-]+@[\w-]+\.[a-zA-Z]{2,}$")) errors.Message.Add("O campo 'Email' está em um formato inválido.");
    if (string.IsNullOrEmpty(adminDto.Senha)) errors.Message.Add("O campo 'Senha' é obrigatório.");
    if (adminDto.Perfil == null) errors.Message.Add("O campo 'Perfil' é obrigatório.");

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
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrador");

app.MapDelete("/administradores/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    veiculoService.Delete(veiculo);
    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrador");


#endregion

#region Veículos

// Método de Validações
ErrorView validaDTO(VeiculoDto veiculoDto)
{
    var errors = new ErrorView
    {
        Message = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDto.Nome)) errors.Message.Add("O campo 'Nome' é obrigatório.");
    if (string.IsNullOrEmpty(veiculoDto.Marca)) errors.Message.Add("O campo 'Marca' é obrigatório.");
    if (veiculoDto.Ano < 1950) errors.Message.Add("O veículo é muito antigo! Só é permitido cadastrar veículo acima do ano de 1950.");

    return errors;
} 

app.MapPost("/veiculos", ([FromBody] VeiculoDto veiculoDto, IVeiculoService veiculoService) =>
{
    // Validações
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

}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Editor" }).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.GetAll(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Editor" }).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Editor" }).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDto veiculoDto, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();

    // Validações
    var errors = validaDTO(veiculoDto);
    if (errors.Message.Count > 0) return Results.BadRequest(errors);

    veiculo.Nome = veiculoDto.Nome;
    veiculo.Marca = veiculoDto.Marca;
    veiculo.Ano = veiculoDto.Ano;

    veiculoService.Update(veiculo);
    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();
    veiculoService.Delete(veiculo);
    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Veiculos");

#endregion

#region App
// Configuração Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Configuração Autenticação
app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion