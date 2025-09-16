using HRMS.DataAccess.Repositories;
using HRMS.Models.DTOs;
using HRMS.Models.Entities;
using HRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IGenericRepository<Employee> _employeeRepository;
    private readonly IGenericRepository<Department> _departmentRepository;

    public EmployeeService(
        IGenericRepository<Employee> employeeRepository,
        IGenericRepository<Department> departmentRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<PagedResult<EmployeeDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
    {
        var paging = request ?? new PagedRequest();

        var query = _employeeRepository
            .Query()
            .Include(employee => employee.Department)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
        {
            var term = $"%{paging.SearchTerm.Trim()}%";
            query = query.Where(employee =>
                EF.Functions.Like(employee.FullName, term) ||
                EF.Functions.Like(employee.EmpNo, term) ||
                EF.Functions.Like(employee.Email, term));
        }

        query = query.OrderBy(employee => employee.FullName);

        var total = await query.CountAsync(cancellationToken);
        var employees = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeDto>
        {
            Items = employees.Select(MapToDto).ToList(),
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var employee = await _employeeRepository
            .Query()
            .Include(entity => entity.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return employee is null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var normalizedEmpNo = NormalizeRequiredString(dto.EmpNo, nameof(dto.EmpNo));
        var fullName = NormalizeRequiredString(dto.FullName, nameof(dto.FullName));
        var email = NormalizeRequiredString(dto.Email, nameof(dto.Email));
        ValidateEmail(email);
        ValidateHireDate(dto.HireDate);

        await EnsureEmployeeNumberIsUniqueAsync(normalizedEmpNo, cancellationToken);
        await EnsureDepartmentExistsAsync(dto.DepartmentId, cancellationToken);

        var employee = new Employee
        {
            EmpNo = normalizedEmpNo,
            FullName = fullName,
            Email = email,
            DepartmentId = dto.DepartmentId,
            HireDate = dto.HireDate
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(employee.Id, cancellationToken)
               ?? MapToDto(employee);
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(dto);

        var employee = await _employeeRepository
            .Query()
            .Include(entity => entity.Department)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var fullName = NormalizeRequiredString(dto.FullName, nameof(dto.FullName));
        var email = NormalizeRequiredString(dto.Email, nameof(dto.Email));
        ValidateEmail(email);
        ValidateHireDate(dto.HireDate);

        if (employee.DepartmentId != dto.DepartmentId)
        {
            var department = await EnsureDepartmentExistsAsync(dto.DepartmentId, cancellationToken);
            employee.DepartmentId = department.Id;
            employee.Department = department;
        }

        employee.FullName = fullName;
        employee.Email = email;
        employee.HireDate = dto.HireDate;

        _employeeRepository.Update(employee);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
               ?? MapToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            return false;
        }

        _employeeRepository.Remove(employee);
        await _employeeRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureEmployeeNumberIsUniqueAsync(string empNo, CancellationToken cancellationToken)
    {
        var normalized = empNo.ToUpperInvariant();
        var exists = await _employeeRepository
            .Query()
            .AnyAsync(employee => employee.EmpNo.ToUpper() == normalized, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Employee number '{empNo}' already exists.");
        }
    }

    private async Task<Department> EnsureDepartmentExistsAsync(int departmentId, CancellationToken cancellationToken)
    {
        if (departmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(departmentId), "Department is required.");
        }

        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        if (department is null)
        {
            throw new InvalidOperationException($"Department '{departmentId}' was not found.");
        }

        return department;
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "The identifier must be greater than zero.");
        }
    }

    private static string NormalizeRequiredString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static void ValidateEmail(string email)
    {
        if (!email.Contains('@', StringComparison.Ordinal) || email.EndsWith('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("A valid email address is required.", nameof(email));
        }
    }

    private static void ValidateHireDate(DateTime hireDate)
    {
        if (hireDate == default)
        {
            throw new ArgumentException("Hire date must be specified.", nameof(hireDate));
        }
    }

    private static EmployeeDto MapToDto(Employee employee) => new()
    {
        Id = employee.Id,
        EmpNo = employee.EmpNo,
        FullName = employee.FullName,
        Email = employee.Email,
        HireDate = employee.HireDate,
        DepartmentName = employee.Department?.Name ?? string.Empty
    };
}
