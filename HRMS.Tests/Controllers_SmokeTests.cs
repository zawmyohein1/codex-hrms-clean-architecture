using HRMS.API.Controllers;
using HRMS.Models.DTOs;
using HRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Tests;

public class Controllers_SmokeTests
{
    [Fact]
    public async Task DepartmentsController_Get_ReturnsOk()
    {
        var service = new StubDepartmentService();
        var controller = new DepartmentsController(service);

        var result = await controller.GetAsync(null, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<PagedResult<DepartmentDto>>(ok.Value);
    }

    [Fact]
    public async Task EmployeesController_GetById_ReturnsNotFoundWhenMissing()
    {
        var service = new StubEmployeeService
        {
            GetByIdHandler = (id, token) => Task.FromResult<EmployeeDto?>(null)
        };
        var controller = new EmployeesController(service);

        var result = await controller.GetByIdAsync(123, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task LeaveBalancesController_Create_ReturnsCreated()
    {
        var service = new StubLeaveBalanceService();
        var controller = new LeaveBalancesController(service);
        var dto = new CreateLeaveBalanceDto
        {
            EmpNo = "EMP001",
            Annual = 10,
            Sick = 5,
            Unpaid = 0
        };

        var result = await controller.CreateAsync(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(LeaveBalancesController.GetByIdAsync), created.ActionName);
    }

    private sealed class StubDepartmentService : IDepartmentService
    {
        public Func<PagedRequest?, CancellationToken, Task<PagedResult<DepartmentDto>>> GetHandler { get; set; }
            = (request, token) => Task.FromResult(new PagedResult<DepartmentDto>
            {
                Items = new[] { new DepartmentDto { Id = 1, Name = "HR" } },
                Total = 1,
                Page = PagedRequest.DefaultPage,
                PageSize = PagedRequest.DefaultPageSize
            });

        public Func<int, CancellationToken, Task<DepartmentDto?>> GetByIdHandler { get; set; }
            = (id, token) => Task.FromResult<DepartmentDto?>(new DepartmentDto { Id = id, Name = "HR" });

        public Func<CreateDepartmentDto, CancellationToken, Task<DepartmentDto>> CreateHandler { get; set; }
            = (dto, token) => Task.FromResult(new DepartmentDto { Id = 1, Name = dto.Name });

        public Func<int, UpdateDepartmentDto, CancellationToken, Task<DepartmentDto?>> UpdateHandler { get; set; }
            = (id, dto, token) => Task.FromResult<DepartmentDto?>(new DepartmentDto { Id = id, Name = dto.Name });

        public Func<int, CancellationToken, Task<bool>> DeleteHandler { get; set; }
            = (id, token) => Task.FromResult(true);

        public Task<PagedResult<DepartmentDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
            => GetHandler(request, cancellationToken);

        public Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdHandler(id, cancellationToken);

        public Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
            => CreateHandler(dto, cancellationToken);

        public Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
            => UpdateHandler(id, dto, cancellationToken);

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
            => DeleteHandler(id, cancellationToken);
    }

    private sealed class StubEmployeeService : IEmployeeService
    {
        public Func<PagedRequest?, CancellationToken, Task<PagedResult<EmployeeDto>>> GetHandler { get; set; }
            = (request, token) => Task.FromResult(new PagedResult<EmployeeDto>
            {
                Items = new[]
                {
                    new EmployeeDto
                    {
                        Id = 1,
                        EmpNo = "EMP001",
                        FullName = "Ada Lovelace",
                        Email = "ada@example.com",
                        DepartmentName = "Engineering",
                        HireDate = new DateTime(2020, 1, 1)
                    }
                },
                Total = 1,
                Page = PagedRequest.DefaultPage,
                PageSize = PagedRequest.DefaultPageSize
            });

        public Func<int, CancellationToken, Task<EmployeeDto?>> GetByIdHandler { get; set; }
            = (id, token) => Task.FromResult<EmployeeDto?>(new EmployeeDto
            {
                Id = id,
                EmpNo = "EMP001",
                FullName = "Ada Lovelace",
                Email = "ada@example.com",
                DepartmentName = "Engineering",
                HireDate = new DateTime(2020, 1, 1)
            });

        public Func<CreateEmployeeDto, CancellationToken, Task<EmployeeDto>> CreateHandler { get; set; }
            = (dto, token) => Task.FromResult(new EmployeeDto
            {
                Id = 2,
                EmpNo = dto.EmpNo,
                FullName = dto.FullName,
                Email = dto.Email,
                DepartmentName = "Engineering",
                HireDate = dto.HireDate
            });

        public Func<int, UpdateEmployeeDto, CancellationToken, Task<EmployeeDto?>> UpdateHandler { get; set; }
            = (id, dto, token) => Task.FromResult<EmployeeDto?>(new EmployeeDto
            {
                Id = id,
                EmpNo = "EMP001",
                FullName = dto.FullName,
                Email = dto.Email,
                DepartmentName = "Engineering",
                HireDate = dto.HireDate
            });

        public Func<int, CancellationToken, Task<bool>> DeleteHandler { get; set; }
            = (id, token) => Task.FromResult(true);

        public Task<PagedResult<EmployeeDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
            => GetHandler(request, cancellationToken);

        public Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdHandler(id, cancellationToken);

        public Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
            => CreateHandler(dto, cancellationToken);

        public Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
            => UpdateHandler(id, dto, cancellationToken);

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
            => DeleteHandler(id, cancellationToken);
    }

    private sealed class StubLeaveBalanceService : ILeaveBalanceService
    {
        public Func<PagedRequest?, CancellationToken, Task<PagedResult<LeaveBalanceDto>>> GetHandler { get; set; }
            = (request, token) => Task.FromResult(new PagedResult<LeaveBalanceDto>
            {
                Items = new[]
                {
                    new LeaveBalanceDto
                    {
                        Id = 1,
                        EmpNo = "EMP001",
                        Annual = 10,
                        Sick = 5,
                        Unpaid = 0
                    }
                },
                Total = 1,
                Page = PagedRequest.DefaultPage,
                PageSize = PagedRequest.DefaultPageSize
            });

        public Func<int, CancellationToken, Task<LeaveBalanceDto?>> GetByIdHandler { get; set; }
            = (id, token) => Task.FromResult<LeaveBalanceDto?>(new LeaveBalanceDto
            {
                Id = id,
                EmpNo = "EMP001",
                Annual = 10,
                Sick = 5,
                Unpaid = 0
            });

        public Func<CreateLeaveBalanceDto, CancellationToken, Task<LeaveBalanceDto>> CreateHandler { get; set; }
            = (dto, token) => Task.FromResult(new LeaveBalanceDto
            {
                Id = 3,
                EmpNo = dto.EmpNo,
                Annual = dto.Annual,
                Sick = dto.Sick,
                Unpaid = dto.Unpaid
            });

        public Func<int, UpdateLeaveBalanceDto, CancellationToken, Task<LeaveBalanceDto?>> UpdateHandler { get; set; }
            = (id, dto, token) => Task.FromResult<LeaveBalanceDto?>(new LeaveBalanceDto
            {
                Id = id,
                EmpNo = "EMP001",
                Annual = dto.Annual,
                Sick = dto.Sick,
                Unpaid = dto.Unpaid
            });

        public Func<int, CancellationToken, Task<bool>> DeleteHandler { get; set; }
            = (id, token) => Task.FromResult(true);

        public Task<PagedResult<LeaveBalanceDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
            => GetHandler(request, cancellationToken);

        public Task<LeaveBalanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdHandler(id, cancellationToken);

        public Task<LeaveBalanceDto?> GetByEmployeeNumberAsync(string empNo, CancellationToken cancellationToken = default)
            => Task.FromResult<LeaveBalanceDto?>(null);

        public Task<LeaveBalanceDto> CreateAsync(CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
            => CreateHandler(dto, cancellationToken);

        public Task<LeaveBalanceDto?> UpdateAsync(int id, UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
            => UpdateHandler(id, dto, cancellationToken);

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
            => DeleteHandler(id, cancellationToken);
    }
}
