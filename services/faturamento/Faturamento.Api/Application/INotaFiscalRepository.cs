using Faturamento.Api.Domain;

namespace Faturamento.Api.Application;

public interface INotaFiscalRepository
{
    Task<List<NotaFiscal>> ListarAsync();
    Task<NotaFiscal?> ObterPorIdAsync(Guid id);
    Task<int> ProximoNumeroAsync();
    Task AdicionarAsync(NotaFiscal nota);
    Task SalvarAsync();
}