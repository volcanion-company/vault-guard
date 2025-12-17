using MediatR;
using Microsoft.Extensions.Logging;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Repositories;

namespace VaultGuard.Application.Features.Vaults.Queries;

public sealed class GetVaultsQueryHandler : IRequestHandler<GetVaultsQuery, IEnumerable<VaultDto>>
{
    private readonly IVaultRepository _vaultRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetVaultsQueryHandler> _logger;

    public GetVaultsQueryHandler(
        IVaultRepository vaultRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<GetVaultsQueryHandler> logger)
    {
        _vaultRepository = vaultRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<VaultDto>> Handle(GetVaultsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"vaults:{_currentUserService.UserId}";

        // Try get from cache
        var cachedVaults = await _cacheService.GetAsync<List<VaultDto>>(cacheKey, cancellationToken);
        if (cachedVaults != null)
        {
            _logger.LogInformation("Returning vaults from cache for user {UserId}", _currentUserService.UserId);
            return cachedVaults;
        }

        _logger.LogInformation("Getting vaults from database for user {UserId}", _currentUserService.UserId);

        // Get from read database
        var vaults = await _vaultRepository.GetByOwnerIdAsync(_currentUserService.UserId, cancellationToken);

        var vaultDtos = vaults
            .Where(v => !v.IsDeleted)
            .Select(v => new VaultDto
            {
                Id = v.Id,
                Name = v.Name,
                Version = v.Version,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                ItemCount = v.Items.Count(i => !i.IsDeleted)
            })
            .ToList();

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, vaultDtos, TimeSpan.FromMinutes(5), cancellationToken);

        return vaultDtos;
    }
}
