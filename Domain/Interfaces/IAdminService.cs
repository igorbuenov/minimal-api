using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdminService
    {
        Administrador? Login(LoginDto loginDto);
        void Insert(Administrador admin);
        List<Administrador> GetAll(int? pagina = 1);
        Administrador? GetById(int id);
        void Update(Administrador administrador);
        void Delete(Administrador administrador);


    }
}
