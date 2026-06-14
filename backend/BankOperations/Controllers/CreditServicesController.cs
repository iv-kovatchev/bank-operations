using BankOperations.DTOs.CreditServices;
using BankOperations.Services.CreditServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/creditservices")]
[Authorize]
public class CreditServicesController : ControllerBase
{
    private readonly ICreditServiceService _creditServiceService;

    public CreditServicesController(ICreditServiceService creditServiceService)
    {
        _creditServiceService = creditServiceService;
    }

    [HttpGet]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _creditServiceService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _creditServiceService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCreditServiceDto dto)
    {
        var result = await _creditServiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCreditServiceDto dto)
    {
        var result = await _creditServiceService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _creditServiceService.DeleteAsync(id);
        return NoContent();
    }
}
