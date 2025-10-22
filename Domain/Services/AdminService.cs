using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infraestructure.DB;

namespace minimal_api.Domain.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _appDbContext;

        public AdminService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Administrador? Login(LoginDto loginDto)
        {
            return _appDbContext.Administradores.Where(admin => admin.Email == loginDto.Email && admin.Senha == loginDto.Senha).FirstOrDefault();
        }
    }
}
