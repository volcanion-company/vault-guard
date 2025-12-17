using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultGuard.Domain.Entities;

namespace VaultGuard.Persistence.Configurations;

public sealed class VaultItemConfiguration : IEntityTypeConfiguration<VaultItem>
{
    public void Configure(EntityTypeBuilder<VaultItem> builder)
    {
        builder.ToTable("VaultItems");

        builder.HasKey(vi => vi.Id);

        builder.Property(vi => vi.VaultId)
            .IsRequired();

        builder.HasIndex(vi => vi.VaultId);

        builder.Property(vi => vi.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.OwnsOne(vi => vi.EncryptedPayload, ep =>
        {
            ep.Property(e => e.CipherText)
                .IsRequired()
                .HasColumnName("EncryptedPayloadCipherText")
                .HasMaxLength(10000);

            ep.Property(e => e.InitializationVector)
                .IsRequired()
                .HasColumnName("EncryptedPayloadIV")
                .HasMaxLength(500);
        });

        builder.Property(vi => vi.Metadata)
            .HasMaxLength(2000);

        builder.Property(vi => vi.Version)
            .IsRequired();

        builder.Property(vi => vi.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(vi => !vi.IsDeleted);

        builder.Property(vi => vi.CreatedAt)
            .IsRequired();

        builder.Property(vi => vi.UpdatedAt);

        builder.Property(vi => vi.LastAccessedAt);
    }
}
