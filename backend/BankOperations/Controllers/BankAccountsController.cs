using System.Security.Claims;
using BankOperations.DTOs.BankAccounts;
using BankOperations.Services.BankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/clients/{clientId}/accounts")]
[Authorize(Roles = "Employee,Admin,Client")]
public class BankAccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;

    public BankAccountsController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
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

        var accounts = await _bankAccountService.GetAllByClientIdAsync(clientId);
        return Ok(accounts);
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> OpenAccount(Guid clientId, [FromBody] CreateBankAccountDto dto)
    {
        var createdByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bankAccountService.OpenAccountAsync(clientId, dto, createdByUserId);
        return CreatedAtAction(nameof(GetAll), new { clientId }, result);
    }

    [HttpPatch("~/api/accounts/{id}/close")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> CloseAccount(Guid id)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        await _bankAccountService.CloseAccountAsync(id, requestingUserId, isAdmin);
        return NoContent();
    }

    [HttpDelete("~/api/accounts/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        await _bankAccountService.DeleteAccountAsync(id);
        return NoContent();
    }

    [HttpPatch("~/api/accounts/{id}/deposit")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] TransactionDto dto)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _bankAccountService.DepositAsync(id, dto.Amount, requestingUserId, isAdmin);
        return Ok(result);
    }

    [HttpPatch("~/api/accounts/{id}/withdraw")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] TransactionDto dto)
    {
        var requestingUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _bankAccountService.WithdrawAsync(id, dto.Amount, requestingUserId, isAdmin);
        return Ok(result);
    }
}
