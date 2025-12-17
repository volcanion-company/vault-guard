using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordVerifier)
            .IsRequired()
            .HasMaxLength(1000);

        builder.OwnsOne(u => u.EncryptedMasterKey, emk =>
        {
            emk.Property(e => e.CipherText)
                .IsRequired()
                .HasColumnName("EncryptedMasterKeyCipherText")
                .HasMaxLength(5000);

            emk.Property(e => e.InitializationVector)
                .IsRequired()
                .HasColumnName("EncryptedMasterKeyIV")
                .HasMaxLength(500);
        });

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasMany(u => u.Vaults)
            .WithOne()
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Devices)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
