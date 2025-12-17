using MediatR;
using Microsoft.AspNetCore.Mvc;
using VaultGuard.Application.Features.VaultItems.Commands;
using VaultGuard.Application.Features.VaultItems.Queries;

namespace VaultGuard.Api.Controllers;

[ApiController]
[Route("api/vaults/{vaultId}/[controller]")]
public sealed class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all items in a vault
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVaultItems([FromRoute] Guid vaultId, CancellationToken cancellationToken)
    {
        var query = new GetVaultItemsQuery { VaultId = vaultId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new item in a vault
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVaultItem([FromRoute] Guid vaultId, [FromBody] CreateVaultItemCommand command, CancellationToken cancellationToken)
    {
        command.VaultId = vaultId;
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVaultItems), new { vaultId = vaultId, id = result.Id }, result);
    }
}
