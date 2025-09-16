using HRMS.DataAccess;
using HRMS.DataAccess.Repositories;
using HRMS.Models.DTOs;
using HRMS.Models.Entities;
using HRMS.Services;
using HRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Tests;

public class Services_SmokeTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task EmployeeService_CanCreateAndRetrieveEmployee()
    {
        await using var context = CreateContext();
        var departmentRepository = new GenericRepository<Department>(context);
        var employeeRepository = new GenericRepository<Employee>(context);

        IDepartmentService departmentService = new DepartmentService(departmentRepository);
        var department = await departmentService.CreateAsync(new CreateDepartmentDto
        {
            Name = "Engineering"
        });

        IEmployeeService employeeService = new EmployeeService(employeeRepository, departmentRepository);
        var createdEmployee = await employeeService.CreateAsync(new CreateEmployeeDto
        {
            EmpNo = "EMP001",
            FullName = "Ada Lovelace",
            Email = "ada@example.com",
            DepartmentId = department.Id,
            HireDate = new DateTime(2020, 1, 1)
        });

        Assert.NotNull(createdEmployee);
        Assert.Equal("Ada Lovelace", createdEmployee.FullName);

        var fetchedEmployee = await employeeService.GetByIdAsync(createdEmployee.Id);
        Assert.NotNull(fetchedEmployee);
        Assert.Equal(department.Name, fetchedEmployee!.DepartmentName);
    }
}
