namespace Estoque.Api.Domain;

public class Produto
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = default!;
    public string Descricao { get; private set; } = default!;
    public int Saldo { get; private set; }

    // Construtor privado sem parâmetros - existe só para o EF Core conseguir
    // reconstruir o objeto a partir dos dados do banco. Ninguém no nosso
    // código chama esse construtor diretamente.
    private Produto() { }

    public Produto(string codigo, string descricao, int saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código é obrigatório.", nameof(codigo));

        if (saldoInicial < 0)
            throw new ArgumentException("Saldo inicial não pode ser negativo.", nameof(saldoInicial));

        Id = Guid.NewGuid();
        Codigo = codigo;
        Descricao = descricao;
        Saldo = saldoInicial;
    }

    public bool BaixarSaldo(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));

        if (Saldo < quantidade)
            return false;

        Saldo -= quantidade;
        return true;
    }
}