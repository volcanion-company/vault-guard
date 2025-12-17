using MediatR;
using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Enums;

namespace VaultGuard.Application.Features.VaultItems.Commands;

/// <summary>
/// Command to create a new vault item
/// </summary>
public sealed class CreateVaultItemCommand : IRequest<VaultItemDto>
{
    public Guid VaultId { get; set; }
    public VaultItemType Type { get; set; }
    public string EncryptedPayloadCipherText { get; set; } = string.Empty;
    public string EncryptedPayloadIV { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
