namespace Faturamento.Api.Domain;

public class NotaFiscal
{
    private readonly List<ItemNotaFiscal> _itens = new();

    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public StatusNotaFiscal Status { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public IReadOnlyCollection<ItemNotaFiscal> Itens => _itens.AsReadOnly();

    private NotaFiscal() { }


    public NotaFiscal(int numero)
    {
        Id = Guid.NewGuid();
        Numero = numero;
        Status = StatusNotaFiscal.Aberta;
        CriadaEm = DateTime.UtcNow;
    }

    public void AdicionarItem(Guid produtoId, string descricaoProduto, int quantidade)
    {
        GarantirQueEstaAberta();
        _itens.Add(new ItemNotaFiscal(produtoId, descricaoProduto, quantidade));
    }

    public void Fechar()
    {
        GarantirQueEstaAberta();

        if (_itens.Count == 0)
            throw new InvalidOperationException("Não é possível fechar uma nota sem itens.");

        Status = StatusNotaFiscal.Fechada;
    }

    private void GarantirQueEstaAberta()
    {
        if (Status != StatusNotaFiscal.Aberta)
            throw new InvalidOperationException("Operação permitida apenas para notas com status Aberta.");
    }
}