using MediatR;
using Microsoft.Extensions.Logging;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Features.VaultItems.Commands;

public sealed class CreateVaultItemCommandHandler(
    IVaultRepository vaultRepository,
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService,
    ILogger<CreateVaultItemCommandHandler> logger) : IRequestHandler<CreateVaultItemCommand, VaultItemDto>
{
    public async Task<VaultItemDto> Handle(CreateVaultItemCommand request, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating vault item in vault {VaultId} for user {UserId}", request.VaultId, currentUserService.UserId);
        }

        // Get vault from write database
        var vault = await vaultRepository.GetByIdAsync(request.VaultId, cancellationToken) ?? throw new UnauthorizedAccessException("Vault not found");

        // Ensure user owns the vault
        vault.EnsureOwnership(currentUserService.UserId);

        // Create encrypted payload value object
        var encryptedPayload = EncryptedData.Create( request.EncryptedPayloadCipherText, request.EncryptedPayloadIV);

        // Add item to vault (domain logic)
        var vaultItem = vault.AddItem(request.Type, encryptedPayload, request.Metadata);

        // Update vault
        vaultRepository.Update(vault);

        // Create audit log
        var auditLog = AuditLog.Create(currentUserService.UserId, AuditAction.VaultItemCreated, $"Item created in vault {vault.Name}", currentUserService.IpAddress, currentUserService.UserAgent);
        await auditLogRepository.AddAsync(auditLog, cancellationToken);

        // Commit transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync($"vault:{request.VaultId}", cancellationToken);
        await cacheService.RemoveByPrefixAsync($"vaults:{currentUserService.UserId}", cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Vault item {ItemId} created successfully", vaultItem.Id);

        return new VaultItemDto
        {
            Id = vaultItem.Id,
            VaultId = vaultItem.VaultId,
            Type = vaultItem.Type,
            EncryptedPayload = $"{vaultItem.EncryptedPayload.CipherText}:{vaultItem.EncryptedPayload.InitializationVector}",
            Metadata = vaultItem.Metadata,
            Version = vaultItem.Version,
            CreatedAt = vaultItem.CreatedAt,
            UpdatedAt = vaultItem.UpdatedAt,
            LastAccessedAt = vaultItem.LastAccessedAt
        };
    }
}
