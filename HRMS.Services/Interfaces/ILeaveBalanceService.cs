using HRMS.Models.DTOs;

namespace HRMS.Services.Interfaces;

public interface ILeaveBalanceService
{
    Task<PagedResult<LeaveBalanceDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default);

    Task<LeaveBalanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<LeaveBalanceDto?> GetByEmployeeNumberAsync(string empNo, CancellationToken cancellationToken = default);

    Task<LeaveBalanceDto> CreateAsync(CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default);

    Task<LeaveBalanceDto?> UpdateAsync(int id, UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
