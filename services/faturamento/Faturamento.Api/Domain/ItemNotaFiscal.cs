namespace Faturamento.Api.Domain;

public class ItemNotaFiscal
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string DescricaoProduto { get; private set; } = default!;
    public int Quantidade { get; private set; }

    private ItemNotaFiscal() { }


    public ItemNotaFiscal(Guid produtoId, string descricaoProduto, int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        DescricaoProduto = descricaoProduto;
        Quantidade = quantidade;
    }
}