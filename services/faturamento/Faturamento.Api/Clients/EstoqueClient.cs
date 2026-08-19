namespace Faturamento.Api.Clients;

public interface IEstoqueClient
{
    Task<bool> BaixarSaldoAsync(Guid produtoId, int quantidade, CancellationToken ct);
}

public class EstoqueClient : IEstoqueClient
{
    private readonly HttpClient _http;

    public EstoqueClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> BaixarSaldoAsync(Guid produtoId, int quantidade, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/produtos/{produtoId}/baixar-saldo",
            new { Quantidade = quantidade },
            ct);

        if (response.IsSuccessStatusCode)
            return true;

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            return false;

        response.EnsureSuccessStatusCode();
        return false;
    }
}