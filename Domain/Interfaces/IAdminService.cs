using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdminService
    {
        Administrador? Login(LoginDto loginDto);
    }
}
