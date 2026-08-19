using Faturamento.Api.Application;
using Faturamento.Api.Clients;
using Faturamento.Api.Infrastructure;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<INotaFiscalRepository, NotaFiscalRepositoryEmMemoria>();

// Cliente HTTP para o Serviço de Estoque, com retry + circuit breaker (Polly).
builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5153/");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Tenta 3 vezes com espera crescente (2s, 4s, 8s) antes de desistir.
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, tentativa => TimeSpan.FromSeconds(Math.Pow(2, tentativa)));

// Depois de 5 falhas seguidas, "abre o circuito" por 30s -
// para de tentar bater no Estoque e falha rápido, sem martelar
// um serviço que já está fora do ar.
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));