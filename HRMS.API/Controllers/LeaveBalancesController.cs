using HRMS.Models.DTOs;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalancesController : ApiControllerBase
{
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly ILogger<LeaveBalancesController> _logger;

    public LeaveBalancesController(ILeaveBalanceService leaveBalanceService, ILogger<LeaveBalancesController> logger)
    {
        _leaveBalanceService = leaveBalanceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int? page = null, [FromQuery(Name = "size")] int? size = null, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = BuildPagedRequest(page, size, search);
            var balances = await _leaveBalanceService.GetAsync(request, cancellationToken);
            return Ok(balances);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id:int}", Name = "GetLeaveBalanceById")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _leaveBalanceService.GetByIdAsync(id, cancellationToken);
            return balance is not null ? Ok(balance) : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _leaveBalanceService.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetLeaveBalanceById", new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return HandleAndLogException(ex, _logger, "creating", "leave balance");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _leaveBalanceService.UpdateAsync(id, dto, cancellationToken);
            return updated is not null ? Ok(updated) : NotFound();
        }
        catch (Exception ex)
        {
            return HandleAndLogException(ex, _logger, "updating", "leave balance");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _leaveBalanceService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }
}
