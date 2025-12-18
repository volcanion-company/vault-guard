using MediatR;
using Microsoft.Extensions.Logging;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Features.Vaults.Commands;

public sealed class CreateVaultCommandHandler : IRequestHandler<CreateVaultCommand, VaultDto>
{
    private readonly IVaultRepository _vaultRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateVaultCommandHandler> _logger;

    public CreateVaultCommandHandler(
        IVaultRepository vaultRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<CreateVaultCommandHandler> logger)
    {
        _vaultRepository = vaultRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<VaultDto> Handle(CreateVaultCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Creating vault for user {UserId}", _currentUserService.UserId);
        }

        // Create encrypted vault key value object
        var encryptedVaultKey = EncryptedData.Create(
            request.EncryptedVaultKeyCipherText,
            request.EncryptedVaultKeyIV);

        // Create vault aggregate
        var vault = Vault.Create(
            _currentUserService.UserId,
            request.Name,
            encryptedVaultKey);

        // Save to write database
        await _vaultRepository.AddAsync(vault, cancellationToken);

        // Create audit log
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            AuditAction.VaultCreated,
            $"Vault '{request.Name}' created",
            _currentUserService.IpAddress,
            _currentUserService.UserAgent);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveByPrefixAsync($"vaults:{_currentUserService.UserId}", cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Vault {VaultId} created successfully for user {UserId}", vault.Id, _currentUserService.UserId);

        return new VaultDto
        {
            Id = vault.Id,
            Name = vault.Name,
            Version = vault.Version,
            CreatedAt = vault.CreatedAt,
            UpdatedAt = vault.UpdatedAt,
            ItemCount = 0
        };
    }
}
