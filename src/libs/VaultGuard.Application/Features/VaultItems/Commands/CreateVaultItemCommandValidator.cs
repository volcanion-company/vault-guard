using FluentValidation;

namespace VaultGuard.Application.Features.VaultItems.Commands;

public sealed class CreateVaultItemCommandValidator : AbstractValidator<CreateVaultItemCommand>
{
    public CreateVaultItemCommandValidator()
    {
        RuleFor(x => x.VaultId)
            .NotEmpty().WithMessage("VaultId is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid vault item type");

        RuleFor(x => x.EncryptedPayloadCipherText)
            .NotEmpty().WithMessage("Encrypted payload cipher text is required");

        RuleFor(x => x.EncryptedPayloadIV)
            .NotEmpty().WithMessage("Encrypted payload IV is required");
    }
}
