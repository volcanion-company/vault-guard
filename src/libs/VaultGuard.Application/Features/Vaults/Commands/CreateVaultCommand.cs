using MediatR;
using VaultGuard.Application.DTOs;

namespace VaultGuard.Application.Features.Vaults.Commands;

/// <summary>
/// Command to create a new vault
/// </summary>
public sealed class CreateVaultCommand : IRequest<VaultDto>
{
    public string Name { get; set; } = string.Empty;
    public string EncryptedVaultKeyCipherText { get; set; } = string.Empty;
    public string EncryptedVaultKeyIV { get; set; } = string.Empty;
}
