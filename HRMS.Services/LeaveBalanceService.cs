using HRMS.DataAccess.Repositories;
using HRMS.Models.DTOs;
using HRMS.Models.Entities;
using HRMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly IGenericRepository<LeaveBalance> _leaveBalanceRepository;

    public LeaveBalanceService(IGenericRepository<LeaveBalance> leaveBalanceRepository)
    {
        _leaveBalanceRepository = leaveBalanceRepository;
    }

    public async Task<PagedResult<LeaveBalanceDto>> GetAsync(PagedRequest? request = null, CancellationToken cancellationToken = default)
    {
        var paging = request ?? new PagedRequest();
        var query = _leaveBalanceRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
        {
            var term = $"%{paging.SearchTerm.Trim()}%";
            query = query.Where(balance => EF.Functions.Like(balance.EmpNo, term));
        }

        query = query.OrderBy(balance => balance.EmpNo);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaveBalanceDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Total = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }

    public async Task<LeaveBalanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var balance = await _leaveBalanceRepository
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return balance is null ? null : MapToDto(balance);
    }

    public async Task<LeaveBalanceDto?> GetByEmployeeNumberAsync(string empNo, CancellationToken cancellationToken = default)
    {
        var normalizedEmpNo = NormalizeEmployeeNumber(empNo);

        var balance = await _leaveBalanceRepository
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.EmpNo.ToUpper() == normalizedEmpNo, cancellationToken);

        return balance is null ? null : MapToDto(balance);
    }

    public async Task<LeaveBalanceDto> CreateAsync(CreateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var empNo = NormalizeEmployeeNumber(dto.EmpNo);
        ValidateLeaveValues(dto.Annual, dto.Sick, dto.Unpaid);

        await EnsureEmployeeNumberIsUniqueAsync(empNo, cancellationToken);

        var balance = new LeaveBalance
        {
            EmpNo = empNo,
            Annual = dto.Annual,
            Sick = dto.Sick,
            Unpaid = dto.Unpaid
        };

        await _leaveBalanceRepository.AddAsync(balance, cancellationToken);
        await _leaveBalanceRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(balance);
    }

    public async Task<LeaveBalanceDto?> UpdateAsync(int id, UpdateLeaveBalanceDto dto, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(dto);
        ValidateLeaveValues(dto.Annual, dto.Sick, dto.Unpaid);

        var balance = await _leaveBalanceRepository
            .Query()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (balance is null)
        {
            return null;
        }

        balance.Annual = dto.Annual;
        balance.Sick = dto.Sick;
        balance.Unpaid = dto.Unpaid;

        _leaveBalanceRepository.Update(balance);
        await _leaveBalanceRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(balance);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var balance = await _leaveBalanceRepository.GetByIdAsync(id, cancellationToken);
        if (balance is null)
        {
            return false;
        }

        _leaveBalanceRepository.Remove(balance);
        await _leaveBalanceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NormalizeEmployeeNumber(string empNo)
    {
        if (string.IsNullOrWhiteSpace(empNo))
        {
            throw new ArgumentException("Employee number is required.", nameof(empNo));
        }

        return empNo.Trim().ToUpperInvariant();
    }

    private async Task EnsureEmployeeNumberIsUniqueAsync(string empNo, CancellationToken cancellationToken)
    {
        var exists = await _leaveBalanceRepository
            .Query()
            .AnyAsync(entity => entity.EmpNo.ToUpper() == empNo, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Leave balance for employee '{empNo}' already exists.");
        }
    }

    private static void ValidateLeaveValues(int annual, int sick, int unpaid)
    {
        if (annual < 0 || sick < 0 || unpaid < 0)
        {
            throw new ArgumentOutOfRangeException("Leave balances cannot be negative.");
        }
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "The identifier must be greater than zero.");
        }
    }

    private static LeaveBalanceDto MapToDto(LeaveBalance balance) => new()
    {
        Id = balance.Id,
        EmpNo = balance.EmpNo,
        Annual = balance.Annual,
        Sick = balance.Sick,
        Unpaid = balance.Unpaid
    };
}
