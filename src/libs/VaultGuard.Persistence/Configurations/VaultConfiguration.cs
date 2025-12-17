using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultGuard.Domain.Entities;

namespace VaultGuard.Persistence.Configurations;

public sealed class VaultConfiguration : IEntityTypeConfiguration<Vault>
{
    public void Configure(EntityTypeBuilder<Vault> builder)
    {
        builder.ToTable("Vaults");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.OwnerId)
            .IsRequired();

        builder.HasIndex(v => v.OwnerId);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(v => v.EncryptedVaultKey, evk =>
        {
            evk.Property(e => e.CipherText)
                .IsRequired()
                .HasColumnName("EncryptedVaultKeyCipherText")
                .HasMaxLength(5000);

            evk.Property(e => e.InitializationVector)
                .IsRequired()
                .HasColumnName("EncryptedVaultKeyIV")
                .HasMaxLength(500);
        });

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(v => !v.IsDeleted);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt);

        builder.HasMany(v => v.Items)
            .WithOne(i => i.Vault)
            .HasForeignKey(i => i.VaultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
