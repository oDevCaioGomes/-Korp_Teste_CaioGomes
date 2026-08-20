using Faturamento.Api.Application;
using Faturamento.Api.Clients;
using Faturamento.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

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

    public record ItemNotaFiscalResponse(Guid ProdutoId, string DescricaoProduto, int Quantidade);
    public record NotaFiscalResponse(Guid Id, int Numero, string Status, DateTime CriadaEm, int QuantidadeItens, List<ItemNotaFiscalResponse> Itens);

    [HttpGet]
public async Task<ActionResult<IEnumerable<NotaFiscalResponse>>> Listar()
{
    var notas = await _repositorio.ListarAsync();

    var resultado = notas
        .OrderByDescending(n => n.Numero)
        .Select(n => new NotaFiscalResponse(
            n.Id, n.Numero, n.Status.ToString(), n.CriadaEm, n.Itens.Count,
            n.Itens.Select(i => new ItemNotaFiscalResponse(i.ProdutoId, i.DescricaoProduto, i.Quantidade)).ToList()))
        .ToList();

    return Ok(resultado);
}
    [HttpPost]
    public async Task<ActionResult<NotaFiscalResponse>> Criar()
    {
        var numero = await _repositorio.ProximoNumeroAsync();
        var nota = new NotaFiscal(numero);

        await _repositorio.AdicionarAsync(nota);
        await _repositorio.SalvarAsync();

        return Ok(new NotaFiscalResponse(nota.Id, nota.Numero, nota.Status.ToString(), nota.CriadaEm, 0, new List<ItemNotaFiscalResponse>()));
    }   public record AdicionarItemRequest(Guid ProdutoId, string DescricaoProduto, int Quantidade);

    [HttpPost("{id:guid}/itens")]
    public async Task<ActionResult> AdicionarItem(Guid id, AdicionarItemRequest request)
    {
        var nota = await _repositorio.ObterPorIdAsync(id);
        if (nota is null)
            return NotFound(new { mensagem = "Nota fiscal não encontrada." });

        nota.AdicionarItem(request.ProdutoId, request.DescricaoProduto, request.Quantidade);
        await _repositorio.SalvarAsync();

        return NoContent();
    }

    [HttpPost("{id:guid}/imprimir")]
    public async Task<IActionResult> Imprimir(Guid id, CancellationToken ct)
    {
        var nota = await _repositorio.ObterPorIdAsync(id);
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
        await _repositorio.SalvarAsync();

        return Ok(new NotaFiscalResponse(
            
    nota.Id, nota.Numero, nota.Status.ToString(), nota.CriadaEm, nota.Itens.Count,
    nota.Itens.Select(i => new ItemNotaFiscalResponse(i.ProdutoId, i.DescricaoProduto, i.Quantidade)).ToList()));
    }
}