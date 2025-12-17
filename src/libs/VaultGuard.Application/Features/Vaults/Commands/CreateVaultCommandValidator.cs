using FluentValidation;

namespace VaultGuard.Application.Features.Vaults.Commands;

public sealed class CreateVaultCommandValidator : AbstractValidator<CreateVaultCommand>
{
    public CreateVaultCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vault name is required")
            .MaximumLength(100).WithMessage("Vault name must not exceed 100 characters");

        RuleFor(x => x.EncryptedVaultKeyCipherText)
            .NotEmpty().WithMessage("Encrypted vault key cipher text is required");

        RuleFor(x => x.EncryptedVaultKeyIV)
            .NotEmpty().WithMessage("Encrypted vault key IV is required");
    }
}
