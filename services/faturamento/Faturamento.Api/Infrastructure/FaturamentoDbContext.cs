using Faturamento.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Infrastructure;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => n.Numero).IsUnique();
            entity.Property(n => n.Status).HasConversion<string>();

           entity.OwnsMany(n => n.Itens, itens =>
{
    itens.WithOwner().HasForeignKey("NotaFiscalId");
    itens.HasKey(i => i.Id);
    itens.Property(i => i.Id).ValueGeneratedNever();
});

entity.Navigation(n => n.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}