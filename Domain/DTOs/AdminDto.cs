using minimal_api.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace minimal_api.Domain.DTOs
{
    public record AdminDto
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Perfil { get; set; } = default!;
    }
}
