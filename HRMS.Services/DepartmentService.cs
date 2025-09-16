using HRMS.DataAccess.Repositories;
using HRMS.Models.DTOs;
using HRMS.Models.Entities;
using HRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IGenericRepository<Department> _departmentRepository;

    public DepartmentService(IGenericRepository<Department> departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<PagedResult<DepartmentDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
    {
        var paging = request ?? new PagedRequest();
        var query = _departmentRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
        {
            var term = paging.SearchTerm.Trim();
            query = query.Where(department => EF.Functions.Like(department.Name, $"%{term}%"));
        }

        query = query.OrderBy(department => department.Name);

        var total = await query.CountAsync(cancellationToken);
        var departments = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentDto>
        {
            Items = departments.Select(MapToDto).ToList(),
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var department = await _departmentRepository
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return department is null ? null : MapToDto(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var name = NormalizeName(dto.Name);

        await EnsureNameIsUniqueAsync(name, cancellationToken);

        var department = new Department
        {
            Name = name
        };

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(department);
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(dto);
        var name = NormalizeName(dto.Name);

        var department = await _departmentRepository
            .Query()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (department is null)
        {
            return null;
        }

        if (!string.Equals(department.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureNameIsUniqueAsync(name, cancellationToken);
        }

        department.Name = name;
        _departmentRepository.Update(department);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(department);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);
        if (department is null)
        {
            return false;
        }

        _departmentRepository.Remove(department);
        await _departmentRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DepartmentDto MapToDto(Department department) => new()
    {
        Id = department.Id,
        Name = department.Name
    };

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.", nameof(name));
        }

        return name.Trim();
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "The identifier must be greater than zero.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();
        var exists = await _departmentRepository
            .Query()
            .AnyAsync(entity => entity.Name.ToUpper() == normalizedName, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Department '{name}' already exists.");
        }
    }
}
