using Estoque.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Infrastructure;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Codigo).IsRequired().HasMaxLength(30);
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Descricao).IsRequired().HasMaxLength(200);
        });
    }
}