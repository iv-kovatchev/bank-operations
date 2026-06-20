using System.Security.Claims;
using BankOperations.DTOs.Credits;
using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;
using BankOperations.Services.Credits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/clients/{clientId}/credits")]
[Authorize(Roles = "Employee,Admin,Client")]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid clientId)
    {
        if (User.IsInRole("Client"))
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (clientId != userId)
                return Forbid();
        }

        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var credits = await _creditService.GetAllByClientIdAsync(clientId, requestingUserId, isAdmin);
        return Ok(credits);
    }

    [HttpGet("~/api/credits/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.GetByIdAsync(id, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpPost("consumer")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> GrantConsumerCredit(Guid clientId, [FromBody] CreateConsumerCreditDto dto)
    {
        dto.ClientId = clientId;
        var createdByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _creditService.GrantConsumerCreditAsync(dto, createdByUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("mortgage")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> GrantMortgageCredit(Guid clientId, [FromBody] CreateMortgageCreditDto dto)
    {
        dto.ClientId = clientId;
        var createdByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _creditService.GrantMortgageCreditAsync(dto, createdByUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("~/api/credits/{id}/consumer")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> UpdateConsumerCredit(Guid id, [FromBody] UpdateConsumerCreditDto dto)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.UpdateConsumerCreditAsync(id, dto, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpPut("~/api/credits/{id}/mortgage")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> UpdateMortgageCredit(Guid id, [FromBody] UpdateMortgageCreditDto dto)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.UpdateMortgageCreditAsync(id, dto, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpGet("~/api/credits/{id}/repayment-plan")]
    [Authorize(Roles = "Employee,Admin,Client")]
    public async Task<IActionResult> GetRepaymentPlan(Guid id)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.GetRepaymentPlanAsync(id, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpPatch("~/api/credits/{creditId}/installments/{installmentId}/pay")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> PayInstallment(Guid creditId, Guid installmentId, [FromBody] PayInstallmentDto dto)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.PayInstallmentAsync(creditId, installmentId, dto.BankAccountId, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpPatch("~/api/credits/{creditId}/installments/{installmentId}/unpay")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> UnpayInstallment(Guid creditId, Guid installmentId)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _creditService.UnpayInstallmentAsync(creditId, installmentId, requestingUserId, isAdmin);
        return Ok(result);
    }
}
