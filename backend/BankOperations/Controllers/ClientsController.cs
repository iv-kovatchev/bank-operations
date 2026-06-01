using System.Security.Claims;
using BankOperations.DTOs.Clients.CorporateClients;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Services.Clients;
using BankOperations.Services.Clients.CorporateClients;
using BankOperations.Services.Clients.IndividualClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employee,Admin")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IIndividualClientService _individualClientService;
    private readonly ICorporateClientService _corporateClientService;

    public ClientsController(
        IClientService clientService,
        IIndividualClientService individualClientService,
        ICorporateClientService corporateClientService)
    {
        _clientService = clientService;
        _individualClientService = individualClientService;
        _corporateClientService = corporateClientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var createdByUserId = User.IsInRole("Employee") ? userId : (Guid?)null;
        var clients = await _clientService.GetAllClientsAsync(createdByUserId);
        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var client = await _clientService.GetClientByIdAsync(id, userId, isAdmin);
        return Ok(client);
    }

    [HttpPost("individual")]
    public async Task<IActionResult> CreateIndividual([FromBody] CreateIndividualClientDto dto)
    {
        var createdByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _individualClientService.CreateAsync(dto, createdByUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("corporate")]
    public async Task<IActionResult> CreateCorporate([FromBody] CreateCorporateClientDto dto)
    {
        var createdByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _corporateClientService.CreateAsync(dto, createdByUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("individual/{id:guid}")]
    public async Task<IActionResult> UpdateIndividual(Guid id, [FromBody] UpdateIndividualClientDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _individualClientService.UpdateAsync(id, dto, userId, isAdmin);
        return Ok(result);
    }

    [HttpPut("corporate/{id:guid}")]
    public async Task<IActionResult> UpdateCorporate(Guid id, [FromBody] UpdateCorporateClientDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _corporateClientService.UpdateAsync(id, dto, userId, isAdmin);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _clientService.DeactivateClientAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _clientService.ActivateClientAsync(id);
        return NoContent();
    }
}
