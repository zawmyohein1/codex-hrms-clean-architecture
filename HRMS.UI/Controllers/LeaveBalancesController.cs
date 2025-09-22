using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using HRMS.Models.DTOs;
using HRMS.UI.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.UI.Controllers;

public class LeaveBalancesController : Controller
{
    private const string LeaveBalancesEndpoint = "api/leavebalances";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LeaveBalancesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(string? searchTerm, string? sortOrder, int page = PagedRequest.DefaultPage, int pageSize = PagedRequest.DefaultPageSize)
    {
        var client = CreateClient();
        var queryParameters = new List<string>();

        if (page > 0)
        {
            queryParameters.Add($"page={page}");
        }

        if (pageSize > 0)
        {
            queryParameters.Add($"size={pageSize}");
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            queryParameters.Add($"search={Uri.EscapeDataString(searchTerm)}");
        }

        var endpoint = LeaveBalancesEndpoint;
        if (queryParameters.Count > 0)
        {
            endpoint = $"{LeaveBalancesEndpoint}?{string.Join("&", queryParameters)}";
        }

        var response = await client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            ViewData["ErrorMessage"] = "Unable to load leave balances at this time.";
            return View(new PagedResult<LeaveBalanceDto>
            {
                Page = page,
                PageSize = pageSize,
                Items = new List<LeaveBalanceDto>()
            });
        }

        var balances = await DeserializeAsync<PagedResult<LeaveBalanceDto>>(response) ?? new PagedResult<LeaveBalanceDto>();
        var currentSort = string.IsNullOrWhiteSpace(sortOrder) ? "empNoAsc" : sortOrder;
        var items = balances.Items ?? Enumerable.Empty<LeaveBalanceDto>();

        balances.Items = currentSort switch
        {
            "empNoDesc" => items.OrderByDescending(balance => balance.EmpNo).ToList(),
            "annualAsc" => items.OrderBy(balance => balance.Annual).ToList(),
            "annualDesc" => items.OrderByDescending(balance => balance.Annual).ToList(),
            "sickAsc" => items.OrderBy(balance => balance.Sick).ToList(),
            "sickDesc" => items.OrderByDescending(balance => balance.Sick).ToList(),
            "unpaidAsc" => items.OrderBy(balance => balance.Unpaid).ToList(),
            "unpaidDesc" => items.OrderByDescending(balance => balance.Unpaid).ToList(),
            _ => items.OrderBy(balance => balance.EmpNo).ToList()
        };

        var resolvedPageSize = balances.PageSize <= 0 ? PagedRequest.DefaultPageSize : balances.PageSize;
        var resolvedPage = balances.Page <= 0 ? PagedRequest.DefaultPage : balances.Page;

        balances.PageSize = resolvedPageSize;
        balances.Page = resolvedPage;

        ViewBag.CurrentSort = currentSort;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPageSize = resolvedPageSize;
        ViewBag.CurrentPage = resolvedPage;

        return View(balances);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{LeaveBalancesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load leave balance details.");
        }

        var balance = await DeserializeAsync<LeaveBalanceDto>(response);
        return balance is not null ? View(balance) : NotFound();
    }

    public IActionResult Create()
    {
        return View(new CreateLeaveBalanceDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLeaveBalanceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PostAsJsonAsync(LeaveBalancesEndpoint, dto);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Leave balance saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        await response.AddErrorsToModelStateAsync(ModelState, "leave balance");
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{LeaveBalancesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load leave balance for editing.");
        }

        var balance = await DeserializeAsync<LeaveBalanceDto>(response);
        if (balance is null)
        {
            return NotFound();
        }

        ViewBag.LeaveBalanceId = balance.Id;
        ViewBag.EmpNo = balance.EmpNo;

        var model = new UpdateLeaveBalanceDto
        {
            Annual = balance.Annual,
            Sick = balance.Sick,
            Unpaid = balance.Unpaid
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateLeaveBalanceDto dto, string? empNo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.LeaveBalanceId = id;
            ViewBag.EmpNo = empNo;
            return View(dto);
        }

        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"{LeaveBalancesEndpoint}/{id}", dto);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Leave balance saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        await response.AddErrorsToModelStateAsync(ModelState, "leave balance");
        ViewBag.LeaveBalanceId = id;
        ViewBag.EmpNo = empNo;
        return View(dto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"{LeaveBalancesEndpoint}/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return Problem("Unable to load leave balance for deletion.");
        }

        var balance = await DeserializeAsync<LeaveBalanceDto>(response);
        if (balance is null)
        {
            return NotFound();
        }

        if (TempData.ContainsKey("ErrorMessage"))
        {
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
        }

        return View(balance);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"{LeaveBalancesEndpoint}/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Unable to delete leave balance.";
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
}
