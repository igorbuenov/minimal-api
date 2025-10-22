using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IVeiculoService
    {
        List<Veiculo> GetAll(int pagina = 1, string? nome = null, string? marca = null);
        Veiculo? GetById(int id);
        void Insert(Veiculo veiculo);
        void Update(Veiculo veiculo);
        void Delete(Veiculo veiculo);
    }
}
