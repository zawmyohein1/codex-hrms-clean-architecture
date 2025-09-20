using System;
using System.Linq;
using HRMS.Models.DTOs;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ApiControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
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

    [HttpGet("{id:int}", Name = "GetDepartmentById")]
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(state => state.Value?.Errors.Count > 0)
                .SelectMany(state => state.Value!.Errors.Select(error => new { Field = state.Key, error.ErrorMessage }))
                .ToList();

            _logger.LogWarning("Invalid department create request: {@ValidationErrors}", errors);
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _departmentService.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetDepartmentById", new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Department creation conflict for name {DepartmentName}", dto.Name);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Department creation failed due to invalid request data.");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Department creation failed due to business rule violation.");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department {DepartmentName}", dto.Name);
            return Problem(detail: "An unexpected error occurred while creating the department.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
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
