using HRMS.Models.DTOs;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ApiControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int? page = null, [FromQuery(Name = "size")] int? size = null, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = BuildPagedRequest(page, size, search);
            var employees = await _employeeService.GetAsync(request, cancellationToken);
            return Ok(employees);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id:int}", Name = "GetEmployeeById")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
            return employee is not null ? Ok(employee) : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _employeeService.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetEmployeeById", new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return HandleAndLogException(ex, _logger, "creating", "employee");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _employeeService.UpdateAsync(id, dto, cancellationToken);
            return updated is not null ? Ok(updated) : NotFound();
        }
        catch (Exception ex)
        {
            return HandleAndLogException(ex, _logger, "updating", "employee");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _employeeService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }
}
