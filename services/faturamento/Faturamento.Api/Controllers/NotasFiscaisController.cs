using Faturamento.Api.Application;
using Faturamento.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Faturamento.Api.Clients;
using Polly.CircuitBreaker;
using System.Net.Http;


namespace Faturamento.Api.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController : ControllerBase
{
private readonly INotaFiscalRepository _repositorio;
private readonly IEstoqueClient _estoqueClient;

public NotasFiscaisController(INotaFiscalRepository repositorio, IEstoqueClient estoqueClient)
{
    _repositorio = repositorio;
    _estoqueClient = estoqueClient;
}

    public record NotaFiscalResponse(Guid Id, int Numero, string Status, DateTime CriadaEm, int QuantidadeItens);
    public record AdicionarItemRequest(Guid ProdutoId, string DescricaoProduto, int Quantidade);

    [HttpGet]
    public ActionResult<IEnumerable<NotaFiscalResponse>> Listar()
    {
        var resultado = _repositorio.Listar()
            .OrderByDescending(n => n.Numero)
            .Select(n => new NotaFiscalResponse(n.Id, n.Numero, n.Status.ToString(), n.CriadaEm, n.Itens.Count))
            .ToList();

        return Ok(resultado);
    }

    [HttpPost]
    public ActionResult<NotaFiscalResponse> Criar()
    {
        var numero = _repositorio.ProximoNumero();
        var nota = new NotaFiscal(numero);
        _repositorio.Adicionar(nota);

        return Ok(new NotaFiscalResponse(nota.Id, nota.Numero, nota.Status.ToString(), nota.CriadaEm, 0));
    }

    [HttpPost("{id:guid}/itens")]
    public ActionResult AdicionarItem(Guid id, AdicionarItemRequest request)
    {
        var nota = _repositorio.ObterPorId(id);
        if (nota is null)
            return NotFound(new { mensagem = "Nota fiscal não encontrada." });

        nota.AdicionarItem(request.ProdutoId, request.DescricaoProduto, request.Quantidade);
        return NoContent();
    }

    [HttpPost("{id:guid}/imprimir")]
public async Task<IActionResult> Imprimir(Guid id, CancellationToken ct)
{
    var nota = _repositorio.ObterPorId(id);
    if (nota is null)
        return NotFound(new { mensagem = "Nota fiscal não encontrada." });

    try
    {
        foreach (var item in nota.Itens)
        {
            var baixou = await _estoqueClient.BaixarSaldoAsync(item.ProdutoId, item.Quantidade, ct);
            if (!baixou)
                return Conflict(new { mensagem = $"Saldo insuficiente para o produto {item.DescricaoProduto}." });
        }
    }
    catch (BrokenCircuitException)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            mensagem = "O serviço de estoque está indisponível no momento (muitas falhas seguidas). Tente novamente em instantes."
        });
    }
    catch (HttpRequestException)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            mensagem = "Não foi possível confirmar o estoque agora. Tente novamente em instantes."
        });
    }

    nota.Fechar();

    return Ok(new NotaFiscalResponse(nota.Id, nota.Numero, nota.Status.ToString(), nota.CriadaEm, nota.Itens.Count));
}
}