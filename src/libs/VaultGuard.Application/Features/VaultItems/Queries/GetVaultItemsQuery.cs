using MediatR;
using VaultGuard.Application.DTOs;

namespace VaultGuard.Application.Features.VaultItems.Queries;

/// <summary>
/// Query to get all items in a vault
/// </summary>
public sealed class GetVaultItemsQuery : IRequest<IEnumerable<VaultItemDto>>
{
    public Guid VaultId { get; set; }
}
