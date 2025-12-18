using MediatR;
using Microsoft.Extensions.Logging;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Repositories;

namespace VaultGuard.Application.Features.VaultItems.Queries;

public sealed class GetVaultItemsQueryHandler(
    IVaultRepository vaultRepository,
    IVaultItemRepository vaultItemRepository,
    ICurrentUserService currentUserService,
    ICacheService cacheService,
    ILogger<GetVaultItemsQueryHandler> logger) : IRequestHandler<GetVaultItemsQuery, IEnumerable<VaultItemDto>>
{
    public async Task<IEnumerable<VaultItemDto>> Handle(GetVaultItemsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"vault:{request.VaultId}:items";

        // Try get from cache
        var cachedItems = await cacheService.GetAsync<List<VaultItemDto>>(cacheKey, cancellationToken);
        if (cachedItems != null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Returning vault items from cache for vault {VaultId}", request.VaultId);
            }
            
            return cachedItems;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Getting vault items from database for vault {VaultId}", request.VaultId);
        }

        // Verify vault ownership
        var vault = await vaultRepository.GetByIdAsync(request.VaultId, cancellationToken) ?? throw new UnauthorizedAccessException("Vault not found");
        vault.EnsureOwnership(currentUserService.UserId);

        // Get items from read database
        var items = await vaultItemRepository.GetByVaultIdAsync(request.VaultId, cancellationToken);

        var itemDtos = items
            .Where(i => !i.IsDeleted)
            .Select(i => new VaultItemDto
            {
                Id = i.Id,
                VaultId = i.VaultId,
                Type = i.Type,
                EncryptedPayload = $"{i.EncryptedPayload.CipherText}:{i.EncryptedPayload.InitializationVector}",
                Metadata = i.Metadata,
                Version = i.Version,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                LastAccessedAt = i.LastAccessedAt
            })
            .ToList();

        // Cache for 5 minutes
        await cacheService.SetAsync(cacheKey, itemDtos, TimeSpan.FromMinutes(5), cancellationToken);

        return itemDtos;
    }
}
