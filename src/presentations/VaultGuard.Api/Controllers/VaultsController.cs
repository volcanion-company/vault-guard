using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultGuard.Application.Features.Vaults.Commands;
using VaultGuard.Application.Features.Vaults.Queries;

namespace VaultGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class VaultsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VaultsController> _logger;

    public VaultsController(IMediator mediator, ILogger<VaultsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all vaults for current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VaultGuard.Application.DTOs.VaultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetVaults(CancellationToken cancellationToken)
    {
        var query = new GetVaultsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new vault
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VaultGuard.Application.DTOs.VaultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateVault([FromBody] CreateVaultCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVaults), new { id = result.Id }, result);
    }
}
