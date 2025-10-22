namespace minimal_api.Domain.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
    }
}
