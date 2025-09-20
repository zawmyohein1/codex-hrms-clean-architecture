using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using HRMS.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.UI.Controllers;

public class DepartmentsController : Controller
{
    private const string DepartmentsEndpoint = "api/departments";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DepartmentsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = CreateClient();
        var response = await client.GetAsync(DepartmentsEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            ViewData["ErrorMessage"] = "Unable to load departments at this time.";
            return View(new PagedResult<DepartmentDto>());
        }

        var departments = await DeserializeAsync<PagedResult<DepartmentDto>>(response) ?? new PagedResult<DepartmentDto>();
        return View(departments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{DepartmentsEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load department details.");
        }

        var department = await DeserializeAsync<DepartmentDto>(response);
        return department is not null ? View(department) : NotFound();
    }

    public IActionResult Create() => View(new CreateDepartmentDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PostAsJsonAsync(DepartmentsEndpoint, dto);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        await AddProblemDetailsToModelStateAsync(response);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{DepartmentsEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load department for editing.");
        }

        var department = await DeserializeAsync<DepartmentDto>(response);
        if (department is null)
        {
            return NotFound();
        }

        ViewBag.DepartmentId = department.Id;
        var model = new UpdateDepartmentDto
        {
            Name = department.Name
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DepartmentId = id;
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"{DepartmentsEndpoint}/{id}", dto);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Unable to update department.");
        ViewBag.DepartmentId = id;
        return View(dto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{DepartmentsEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load department for deletion.");
        }

        var department = await DeserializeAsync<DepartmentDto>(response);
        if (department is null)
        {
            return NotFound();
        }

        if (TempData.ContainsKey("ErrorMessage"))
        {
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
        }

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"{DepartmentsEndpoint}/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Unable to delete department.";
        return RedirectToAction(nameof(Delete), new { id });
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("HRMSApi");

    private async Task AddProblemDetailsToModelStateAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var errorAdded = false;

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.TryGetProperty("title", out var titleElement))
                {
                    var title = titleElement.GetString();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        ModelState.AddModelError(string.Empty, title);
                        errorAdded = true;
                    }
                }

                if (root.TryGetProperty("detail", out var detailElement))
                {
                    var detail = detailElement.GetString();
                    if (!string.IsNullOrWhiteSpace(detail))
                    {
                        ModelState.AddModelError(string.Empty, detail);
                        errorAdded = true;
                    }
                }

                if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in errorsElement.EnumerateObject())
                    {
                        if (property.Value.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var errorElement in property.Value.EnumerateArray())
                        {
                            var message = errorElement.GetString();
                            if (string.IsNullOrWhiteSpace(message))
                            {
                                continue;
                            }

                            ModelState.AddModelError(property.Name, message);
                            errorAdded = true;
                        }
                    }
                }

                if (errorAdded)
                {
                    return;
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors and fall back to a generic message.
            }
        }

        ModelState.AddModelError(string.Empty, "Unable to create department.");
    }

    private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? default
            : JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }
}
