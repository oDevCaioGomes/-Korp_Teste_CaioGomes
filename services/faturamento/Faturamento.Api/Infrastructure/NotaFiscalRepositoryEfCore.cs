using Faturamento.Api.Application;
using Faturamento.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Infrastructure;

public class NotaFiscalRepositoryEfCore : INotaFiscalRepository
{
    private readonly FaturamentoDbContext _db;

    public NotaFiscalRepositoryEfCore(FaturamentoDbContext db)
    {
        _db = db;
    }

    public Task<List<NotaFiscal>> ListarAsync() =>
        _db.NotasFiscais
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();

    public Task<NotaFiscal?> ObterPorIdAsync(Guid id) =>
        _db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

    public async Task<int> ProximoNumeroAsync()
    {
        var maiorNumero = await _db.NotasFiscais
            .Select(n => (int?)n.Numero)
            .MaxAsync();

        return (maiorNumero ?? 0) + 1;
    }

    public async Task AdicionarAsync(NotaFiscal nota) =>
        await _db.NotasFiscais.AddAsync(nota);

    public Task SalvarAsync() => _db.SaveChangesAsync();
}