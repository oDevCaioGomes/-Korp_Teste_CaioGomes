# Registro de problemas encontrados durante o desenvolvimento

## Ambiente

- **WSL desativado sem necessidade real** — o projeto não depende de WSL; SDK do
  .NET e Node/Angular rodam nativamente no Windows. Migrado sem perda de trabalho.
- **Pasta do repositório criada com um espaço no início do nome**
  (`" Korp_Teste_CaioGomes"`) — não impediu o desenvolvimento, mas exigiu contornos
  no terminal (`Get-ChildItem` em vez de digitar o nome direto). Correção definitiva
  pendente: renomear com o VS Code fechado, já que o editor mantém a pasta "travada".

## Estrutura do projeto

- **Template `dotnet new webapi` gera Minimal API por padrão** (desde o .NET 8),
  não o estilo com Controllers. Os dois serviços precisaram ser regenerados com a
  flag `-controllers`.
- **Arquivo `Program.cs` do Estoque editado em uma aba "fantasma"** — aberta a
  partir do diretório errado — deixou o arquivo real desatualizado (faltando
  `var app = builder.Build();`) por várias mensagens, sem erro aparente até tentar
  rodar.
- **Conteúdo colado no arquivo errado** mais de uma vez — código C# parando dentro
  de arquivos `.http`, e vice-versa. Causa raiz: várias abas parecidas abertas ao
  mesmo tempo, sem confirmar qual estava em foco antes de colar.

## Processo / ferramentas

- **`dotnet run` não recarrega sozinho** ao salvar um arquivo — diferente do que eu
  informei inicialmente. Corrigido trocando para `dotnet watch run` a partir de um
  certo ponto.
- **Conflito de arquivo `.pdb`** (`CS2012`) ao rodar `dotnet build` manual enquanto
  um `dotnet watch run` do mesmo projeto já estava ativo — dois processos escrevendo
  no mesmo artefato de build ao mesmo tempo.
- **Dados em memória some a cada restart do servidor** — não é bug, é a natureza do
  repositório em memória (ainda sem banco real), mas gerou confusão repetida durante
  os testes (produtos e notas "sumindo" entre uma chamada e outra).

## Bug de código real (corrigido)

- **Endpoint `imprimir` só tratava `BrokenCircuitException`**, a exceção que o Polly
  lança com o circuito já aberto (após 5 falhas). Nas primeiras falhas — antes do
  circuito abrir — a exceção `HttpRequestException` (ex: conexão recusada) não era
  capturada, vazando um `500` com stack trace completo na resposta.
  **Correção:** adicionado um segundo `catch (HttpRequestException)`, devolvendo a
  mesma resposta amigável (503) independentemente de qual seja a causa da falha.

## Limitação conhecida, ainda não corrigida

- Se a nota tiver múltiplos itens e a baixa de saldo falhar no meio da lista (não no
  primeiro item), os itens anteriores já baixaram saldo no Estoque mas a nota não
  fecha — inconsistência que exigiria uma transação distribuída ou passo de
  compensação para resolver de verdade. Fora do escopo desta fase do teste.
- `EstoqueClient` trata `404` (produto inexistente) e falha de conexão da mesma
  forma — ambos viram "não foi possível confirmar o estoque". Deveriam ter
  tratamento e mensagens diferentes, já que um é erro de dado e o outro é de
  disponibilidade.

  HTTP/1.1 200 OK
Connection: close
Content-Type: application/json; charset=utf-8
Date: Wed, 19 Aug 2026 01:17:19 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "id": "731138d2-293b-470f-9a22-fd618a775744",
  "numero": 1,
  "status": "Fechada",
  "criadaEm": "2026-08-19T01:10:35.7025277",
  "quantidadeItens": 1
}
