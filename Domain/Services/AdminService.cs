using Microsoft.EntityFrameworkCore;
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
            return _appDbContext.Administradores.Where(admin => admin.Email == loginDto.Email && admin.Senha == loginDto.Password).FirstOrDefault();
        }

        public void Insert(Administrador admin)
        {
            _appDbContext.Administradores.Add(admin);
            _appDbContext.SaveChanges();
        }

        public List<Administrador> GetAll(int? pagina = 1)
        {
            var query = _appDbContext.Administradores.AsQueryable();

            int pageSize = 10;

            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * pageSize).Take(pageSize);
            }

            return query.ToList();
        }

        public Administrador? GetById(int id)
        {
            return _appDbContext.Administradores.Where(admin => admin.Id == id).FirstOrDefault();
        }

        public void Update(Administrador administrador)
        {
            _appDbContext.Administradores.Update(administrador);
            _appDbContext.SaveChanges();
        }

        public void Delete(Administrador administrador)
        {
            _appDbContext.Administradores.Remove(administrador);
            _appDbContext.SaveChanges();
        }
    }
}
