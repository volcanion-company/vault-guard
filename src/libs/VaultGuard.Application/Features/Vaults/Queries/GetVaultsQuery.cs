using MediatR;
using VaultGuard.Application.DTOs;

namespace VaultGuard.Application.Features.Vaults.Queries;

/// <summary>
/// Query to get all vaults for current user
/// </summary>
public sealed class GetVaultsQuery : IRequest<IEnumerable<VaultDto>>
{
}
