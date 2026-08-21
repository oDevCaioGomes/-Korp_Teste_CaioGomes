# Detalhamento técnico — Sistema de Emissão de Notas Fiscais

## 1. Frontend (Angular)

### Ciclos de vida utilizados

No código final, os três componentes de tela (`Produtos`, `NotasFiscais`, `NotaFiscalDetalhe`)
usam **`ngOnInit`** para disparar a carga inicial de dados.

`ngOnDestroy` foi usado numa versão intermediária do `NotasFiscaisComponent`, junto com
`Subscription` manual, mas foi removido depois da migração para `signal` (ver seção de RxJS
abaixo). Como as chamadas HTTP feitas via `HttpClient` emitem um único valor e completam
sozinhas, não há vazamento de assinatura nesses casos — diferente do que aconteceria com um
Observable de longa duração (ex: WebSocket, `interval`), onde `ngOnDestroy` seria obrigatório.

### Uso do RxJS

Dois padrões distintos foram usados de propósito, para cobrir os dois casos comuns:

1. **`async` pipe com stream de recarregamento** (`ProdutosComponent`): um `Subject<void>`
   (`recarregar$`) é combinado com `switchMap` e `startWith` para formar o Observable
   `produtos$`, consumido no template via `| async`. Toda vez que `recarregar$.next()` é
   chamado (ex: depois de cadastrar um produto), a lista é buscada de novo automaticamente.
   O Angular gerencia a inscrição/cancelamento sozinho.

2. **`subscribe` manual + `signal`** (`NotasFiscaisComponent`, `NotaFiscalDetalheComponent`):
   chamadas HTTP pontuais (`listar()`, `criar()`, `imprimir()`) são assinadas diretamente,
   atualizando um `signal` com `.set()`.

**Bug real encontrado e corrigido:** na primeira versão do `NotasFiscaisComponent`, os dados
eram atribuídos direto a uma propriedade `NotaFiscal[]` comum dentro do `subscribe`. O Angular
não detectava a mudança — os dados chegavam (confirmado via `console.log`), mas a tabela
continuava vazia. A correção foi trocar a propriedade por um `signal<NotaFiscal[]>([])`,
atualizado via `.set()`, que notifica o Angular explicitamente sobre a mudança de estado.

### Outras bibliotecas

- **`@angular/forms`** (`FormsModule`) — two-way binding (`[(ngModel)]`) nos formulários de
  cadastro de produto e adição de item à nota.
- **`@angular/router`** — navegação entre telas, rota com parâmetro (`/notas-fiscais/:id`) na
  tela de detalhe, `RouterLink`/`RouterLinkActive` no menu lateral.
- **RxJS** — nativo do Angular, usado conforme descrito acima.

Nenhuma biblioteca de terceiros além dessas (sem gerenciador de estado como NgRx — desnecessário
pelo tamanho da aplicação).

### Componentes visuais

**Nenhuma biblioteca de componentes de terceiros** (sem Angular Material, sem PrimeNG). Todo o
sistema visual foi construído à mão, em CSS puro, com tokens de design em variáveis CSS (cores,
tipografia, espaçamento) — seguindo a paleta e a tipografia definidas num mockup HTML/CSS
produzido antes de iniciar o código Angular. Decisão deliberada: manter o bundle enxuto e ter
controle total sobre o visual, incluindo o elemento de assinatura do projeto — os badges de
status em formato de carimbo (borda tracejada, levemente rotacionado).

## 2. Backend (C#)

### Frameworks

- **ASP.NET Core Web API** (.NET 10), com **Controllers** (não Minimal API) — decisão
  deliberada para organizar o código em camadas (`Domain` / `Application` / `Infrastructure` /
  `Controllers`).
- **Entity Framework Core 10**, provider **SQLite**, para persistência real.
- **Polly** (via `Microsoft.Extensions.Http.Polly`) para resiliência na chamada HTTP do
  Faturamento ao Estoque.
- **Microsoft.AspNetCore.OpenApi** para documentação da API (ativo só em ambiente
  Development).

Gerenciamento de dependências via **NuGet** (`PackageReference` no `.csproj`). Este projeto não
usa Go/Golang — implementado inteiramente em C#.

### Tratamento de erros e exceções

Sem middleware global de tratamento de exceções — cada endpoint trata pontualmente as
situações de negócio previstas, retornando o status HTTP correspondente:

| Situação | Status |
|---|---|
| Recurso não encontrado (produto, nota) | `404 Not Found` |
| Código de produto duplicado | `409 Conflict` |
| Saldo insuficiente | `409 Conflict` |
| Tentativa de fechar nota sem itens | `400 Bad Request` |
| Estoque indisponível (circuito aberto ou falha de conexão) | `503 Service Unavailable` |

**Dois bugs reais de tratamento de erro** foram encontrados e corrigidos durante o
desenvolvimento (histórico completo em `docs/registro-de-problemas.md`):

1. Nas primeiras falhas de conexão com o Estoque — antes do circuito do Polly abrir — a
   exceção `HttpRequestException` não era capturada (só `BrokenCircuitException` era tratada),
   vazando um `500` com stack trace completo na resposta.
2. A exceção de domínio lançada por `NotaFiscal.Fechar()` quando a nota não tem itens não era
   capturada no endpoint de impressão, também vazando `500`.

**Limitação conhecida:** sem middleware global, uma exceção verdadeiramente inesperada (fora
das situações mapeadas acima) ainda resultaria numa resposta de erro genérica do ASP.NET Core.
Ficou fora do escopo desta fase.

### Uso de LINQ

- **Estoque** (`ProdutoRepositoryEfCore`): `OrderBy(p => p.Codigo)` na listagem;
  `FirstOrDefaultAsync` com comparação case-insensitive
  (`p.Codigo.ToLower() == codigo.ToLower()`) na busca por código duplicado.
  `ProdutosController` usa `Select` para projetar entidades em DTOs de resposta.
- **Faturamento** (`NotaFiscalRepositoryEfCore`): `MaxAsync` para calcular o próximo número
  sequencial da nota. `NotasFiscaisController` usa `OrderByDescending(n => n.Numero)`,
  `Select` (com projeção aninhada para os itens) e `ToList()` na listagem, além de `foreach`
  sobre `nota.Itens` no fluxo de impressão para chamar o Estoque item a item.

## 3. Persistência de dados

SQLite via EF Core Code First, **uma base de dados por serviço** (`estoque.db`,
`faturamento.db`) — sem acoplamento de banco entre os microsserviços. Migrations geradas com
`dotnet ef migrations add` e **aplicadas automaticamente na inicialização** da aplicação
(`db.Database.Migrate()` logo após `builder.Build()`) — isso garante que, tanto localmente
quanto num container Docker com volume vazio, o schema é criado sem passo manual.

Testado sobrevivendo a: restart do processo, restart do container, e restart completo do
Docker Desktop — persistência real confirmada em todos os casos.

## 4. Arquitetura de microsserviços e comunicação

Dois serviços independentes — **Estoque** (produtos e saldo) e **Faturamento** (notas
fiscais) — cada um com seu próprio banco de dados e `Dockerfile`. Comunicação síncrona via
HTTP REST, sempre em uma direção (Faturamento chama Estoque, nunca o contrário).

Localmente, os serviços se encontram via `localhost` com portas fixas (`5153` e `5010`). Em
Docker Compose, por **nome de serviço** na rede interna criada automaticamente pelo Compose
(`http://estoque-api:8080/`), configurado via variável de ambiente (`Servicos__Estoque`) sem
precisar alterar código — a mesma aplicação funciona nos dois ambientes.

## 5. Tratamento de falhas (requisito obrigatório)

Implementado com **Polly** no `HttpClient` tipado do Faturamento (`IEstoqueClient`):

- **Retry** com backoff exponencial: 3 tentativas (2s, 4s, 8s).
- **Circuit breaker**: abre após 5 falhas seguidas, permanece aberto por 30s.

**Cenário testado e comprovado em dois ambientes diferentes:**

1. Localmente, parando o processo `dotnet` do Estoque (`Ctrl+C`).
2. Em containers Docker, com `docker compose stop estoque-api` / `start estoque-api`.

Em ambos os casos: com o Estoque fora do ar, a tentativa de imprimir retorna `503` com
mensagem clara ("Não foi possível confirmar o estoque agora...") sem travar a aplicação nem
vazar stack trace. Assim que o Estoque volta, a próxima tentativa de impressão funciona
normalmente — sem precisar reiniciar o Faturamento.

## 6. Limitações conhecidas

- Falha parcial ao imprimir nota com múltiplos itens: se o segundo item falhar na baixa de
  saldo, o primeiro já foi baixado no Estoque, mas a nota não fecha — sem transação
  distribuída/compensação.
- `EstoqueClient` trata `404` (produto inexistente) e falha de conexão da mesma forma.
- Sem middleware global de tratamento de exceções (detalhado na seção 2).

Histórico completo de problemas de ambiente e bugs encontrados durante o desenvolvimento em
`docs/registro-de-problemas.md`.

## 7. Requisitos opcionais

- [ ] Tratamento de concorrência — não implementado
- [ ] Uso de Inteligência Artificial — não implementado
- [ ] Idempotência — não implementado

Nenhum foi priorizado nesta fase: o foco foi garantir os três requisitos **obrigatórios**
(microsserviços, banco de dados real, tratamento de falhas) funcionando de ponta a ponta e
testados de verdade — em dois ambientes (local e Docker) — em vez de espalhar esforço em itens
opcionais.