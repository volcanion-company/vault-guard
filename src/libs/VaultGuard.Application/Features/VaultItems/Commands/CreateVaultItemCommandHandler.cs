using MediatR;
using Microsoft.Extensions.Logging;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Features.VaultItems.Commands;

public sealed class CreateVaultItemCommandHandler : IRequestHandler<CreateVaultItemCommand, VaultItemDto>
{
    private readonly IVaultRepository _vaultRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateVaultItemCommandHandler> _logger;

    public CreateVaultItemCommandHandler(
        IVaultRepository vaultRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<CreateVaultItemCommandHandler> logger)
    {
        _vaultRepository = vaultRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<VaultItemDto> Handle(CreateVaultItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating vault item in vault {VaultId} for user {UserId}", 
            request.VaultId, _currentUserService.UserId);

        // Get vault from write database
        var vault = await _vaultRepository.GetByIdAsync(request.VaultId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Vault not found");

        // Ensure user owns the vault
        vault.EnsureOwnership(_currentUserService.UserId);

        // Create encrypted payload value object
        var encryptedPayload = EncryptedData.Create(
            request.EncryptedPayloadCipherText,
            request.EncryptedPayloadIV);

        // Add item to vault (domain logic)
        var vaultItem = vault.AddItem(request.Type, encryptedPayload, request.Metadata);

        // Update vault
        _vaultRepository.Update(vault);

        // Create audit log
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            AuditAction.VaultItemCreated,
            $"Item created in vault {vault.Name}",
            _currentUserService.IpAddress,
            _currentUserService.UserAgent);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPrefixAsync($"vault:{request.VaultId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync($"vaults:{_currentUserService.UserId}", cancellationToken);

        _logger.LogInformation("Vault item {ItemId} created successfully", vaultItem.Id);

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
