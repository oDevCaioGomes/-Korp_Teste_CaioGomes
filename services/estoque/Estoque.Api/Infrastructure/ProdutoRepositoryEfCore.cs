using Estoque.Api.Application;
using Estoque.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Infrastructure;

public class ProdutoRepositoryEfCore : IProdutoRepository
{
    private readonly EstoqueDbContext _db;

    public ProdutoRepositoryEfCore(EstoqueDbContext db)
    {
        _db = db;
    }

    public Task<List<Produto>> ListarAsync() =>
        _db.Produtos.OrderBy(p => p.Codigo).ToListAsync();

    public Task<Produto?> ObterPorIdAsync(Guid id) =>
        _db.Produtos.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Produto?> ObterPorCodigoAsync(string codigo) =>
        _db.Produtos.FirstOrDefaultAsync(p =>
            p.Codigo.ToLower() == codigo.ToLower());

    public async Task AdicionarAsync(Produto produto) =>
        await _db.Produtos.AddAsync(produto);

    public Task SalvarAsync() => _db.SaveChangesAsync();
}