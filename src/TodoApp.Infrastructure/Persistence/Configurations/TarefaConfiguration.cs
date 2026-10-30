using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Tarefas;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public sealed class TarefaConfiguration : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("tarefas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(t => t.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasColumnType("varchar")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.DataVencimento)
            .HasColumnName("data_vencimento")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tarefas_status");

        builder.HasIndex(t => t.DataVencimento)
            .HasDatabaseName("ix_tarefas_data_vencimento");
    }
}
