using Estoque.Api.Application;
using Estoque.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoRepository _repositorio;

    public ProdutosController(IProdutoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public record ProdutoResponse(Guid Id, string Codigo, string Descricao, int Saldo);
    public record CriarProdutoRequest(string Codigo, string Descricao, int SaldoInicial);
    public record BaixarSaldoRequest(int Quantidade);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> Listar()
    {
        var produtos = await _repositorio.ListarAsync();

        var resultado = produtos
            .Select(p => new ProdutoResponse(p.Id, p.Codigo, p.Descricao, p.Saldo))
            .ToList();

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponse>> Criar(CriarProdutoRequest request)
    {
        var existente = await _repositorio.ObterPorCodigoAsync(request.Codigo);
        if (existente is not null)
            return Conflict(new { mensagem = $"Já existe um produto com o código '{request.Codigo}'." });

        var produto = new Produto(request.Codigo, request.Descricao, request.SaldoInicial);
        await _repositorio.AdicionarAsync(produto);
        await _repositorio.SalvarAsync();

        var response = new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
        return Ok(response);
    }

    [HttpPost("{id:guid}/baixar-saldo")]
    public async Task<ActionResult> BaixarSaldo(Guid id, BaixarSaldoRequest request)
    {
        var produto = await _repositorio.ObterPorIdAsync(id);
        if (produto is null)
            return NotFound(new { mensagem = "Produto não encontrado." });

        var conseguiu = produto.BaixarSaldo(request.Quantidade);
        if (!conseguiu)
            return Conflict(new { mensagem = "Saldo insuficiente para a quantidade solicitada." });

        await _repositorio.SalvarAsync();
        return NoContent();
    }
}