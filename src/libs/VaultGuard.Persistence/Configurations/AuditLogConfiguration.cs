using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultGuard.Domain.Entities;

namespace VaultGuard.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.UserId)
            .IsRequired();

        builder.HasIndex(al => new { al.UserId, al.CreatedAt });

        builder.Property(al => al.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(al => al.Metadata)
            .HasMaxLength(2000);

        builder.Property(al => al.IpAddress)
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.CreatedAt)
            .IsRequired();
    }
}
