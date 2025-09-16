using HRMS.Models.DTOs;

namespace HRMS.Services.Interfaces;

public interface IDepartmentService
{
    Task<PagedResult<DepartmentDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default);

    Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
