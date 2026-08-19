using Faturamento.Api.Application;
using Faturamento.Api.Domain;

namespace Faturamento.Api.Infrastructure;

public class NotaFiscalRepositoryEmMemoria : INotaFiscalRepository
{
    private readonly List<NotaFiscal> _notas = new();

    public List<NotaFiscal> Listar() => _notas;

    public NotaFiscal? ObterPorId(Guid id) =>
        _notas.FirstOrDefault(n => n.Id == id);

    public int ProximoNumero() =>
        _notas.Count == 0 ? 1 : _notas.Max(n => n.Numero) + 1;

    public void Adicionar(NotaFiscal nota) => _notas.Add(nota);
}