using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infraestructure.DB;

namespace minimal_api.Domain.Services
{
    public class VeiculoService : IVeiculoService
    {

        private readonly AppDbContext _appDbContext;

        public VeiculoService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public void Delete(Veiculo veiculo)
        {
            _appDbContext.Veiculos.Remove(veiculo);
            _appDbContext.SaveChanges();
        }

        public List<Veiculo> GetAll(int pagina = 1, string? nome = null, string? marca = null)
        {

            var query = _appDbContext.Veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v =>  EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));
            }

            int pageSize = 10;

            query = query.Skip((pagina - 1) * pageSize).Take(pageSize);

            return query.ToList();
        }

        public Veiculo? GetById(int id)
        {
            return _appDbContext.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Insert(Veiculo veiculo)
        {
            _appDbContext.Veiculos.Add(veiculo);
            _appDbContext.SaveChanges();
        }

        public void Update(Veiculo veiculo)
        {
            _appDbContext.Veiculos.Update(veiculo);
            _appDbContext.SaveChanges();
        }
    }
}
