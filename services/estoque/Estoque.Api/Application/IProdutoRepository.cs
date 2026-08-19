using Estoque.Api.Domain;

namespace Estoque.Api.Application;

public interface IProdutoRepository
{
    Task<List<Produto>> ListarAsync();
    Task<Produto?> ObterPorIdAsync(Guid id);
    Task<Produto?> ObterPorCodigoAsync(string codigo);
    Task AdicionarAsync(Produto produto);
    Task SalvarAsync();
}