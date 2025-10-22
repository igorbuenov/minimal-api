using minimal_api.Domain.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ola Mundo!");


app.MapPost("/login", (LoginDto loginDto) =>
{
    if (loginDto.Email == "adm@teste.com" && loginDto.Senha == "123456")
    {
        return Results.Ok("Admin logado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
});






app.Run();
