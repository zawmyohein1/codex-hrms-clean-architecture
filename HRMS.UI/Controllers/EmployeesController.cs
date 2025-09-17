using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using HRMS.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.UI.Controllers;

public class EmployeesController : Controller
{
    private const string EmployeesEndpoint = "api/employees";
    private const string DepartmentsEndpoint = "api/departments";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmployeesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = CreateClient();
        var response = await client.GetAsync(EmployeesEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            ViewData["ErrorMessage"] = "Unable to load employees at this time.";
            return View(new PagedResult<EmployeeDto>());
        }

        var employees = await DeserializeAsync<PagedResult<EmployeeDto>>(response) ?? new PagedResult<EmployeeDto>();
        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{EmployeesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load employee details.");
        }

        var employee = await DeserializeAsync<EmployeeDto>(response);
        return employee is not null ? View(employee) : NotFound();
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDepartmentsAsync();
        var model = new CreateEmployeeDto
        {
            HireDate = DateTime.Today
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PostAsJsonAsync(EmployeesEndpoint, dto);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to create employee.");
        await PopulateDepartmentsAsync(dto.DepartmentId);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{EmployeesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load employee for editing.");
        }

        var employee = await DeserializeAsync<EmployeeDto>(response);
        if (employee is null)
        {
            return NotFound();
        }

        var departments = await FetchDepartmentsAsync();
        var selectedDepartmentId = departments
            .FirstOrDefault(d => string.Equals(d.Name, employee.DepartmentName, StringComparison.OrdinalIgnoreCase))?.Id;

        ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartmentId);
        ViewBag.EmployeeId = employee.Id;
        ViewBag.EmpNo = employee.EmpNo;

        var model = new UpdateEmployeeDto
        {
            FullName = employee.FullName,
            Email = employee.Email,
            DepartmentId = selectedDepartmentId ?? 0,
            HireDate = employee.HireDate
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateEmployeeDto dto, string? empNo)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(dto.DepartmentId);
            ViewBag.EmployeeId = id;
            ViewBag.EmpNo = empNo;
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"{EmployeesEndpoint}/{id}", dto);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to update employee.");
        await PopulateDepartmentsAsync(dto.DepartmentId);
        ViewBag.EmployeeId = id;
        ViewBag.EmpNo = empNo;
        return View(dto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{EmployeesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load employee for deletion.");
        }

        var employee = await DeserializeAsync<EmployeeDto>(response);
        if (employee is null)
        {
            return NotFound();
        }

        if (TempData.ContainsKey("ErrorMessage"))
        {
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
        }

        return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"{EmployeesEndpoint}/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Unable to delete employee.";
        return RedirectToAction(nameof(Delete), new { id });
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("HRMSApi");

    private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? default
            : JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    private async Task PopulateDepartmentsAsync(int? selectedId = null)
    {
        var departments = await FetchDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedId);
    }

    private async Task<List<DepartmentDto>> FetchDepartmentsAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync(DepartmentsEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            return new List<DepartmentDto>();
        }

        var pagedDepartments = await DeserializeAsync<PagedResult<DepartmentDto>>(response);
        return pagedDepartments?.Items?.ToList() ?? new List<DepartmentDto>();
    }
}
