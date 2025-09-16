using HRMS.Models.DTOs;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ApiControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] int? page = null, [FromQuery(Name = "size")] int? size = null, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = BuildPagedRequest(page, size, search);
            var departments = await _departmentService.GetAsync(request, cancellationToken);
            return Ok(departments);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _departmentService.GetByIdAsync(id, cancellationToken);
            return department is not null ? Ok(department) : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _departmentService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _departmentService.UpdateAsync(id, dto, cancellationToken);
            return updated is not null ? Ok(updated) : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _departmentService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return HandleException(ex);
        }
    }
}
