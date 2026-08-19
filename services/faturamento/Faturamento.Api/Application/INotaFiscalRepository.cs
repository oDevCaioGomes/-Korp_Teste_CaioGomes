using Faturamento.Api.Domain;

namespace Faturamento.Api.Application;

public interface INotaFiscalRepository
{
    List<NotaFiscal> Listar();
    NotaFiscal? ObterPorId(Guid id);
    int ProximoNumero();
    void Adicionar(NotaFiscal nota);
}