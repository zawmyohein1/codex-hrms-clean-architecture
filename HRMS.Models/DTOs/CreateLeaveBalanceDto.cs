namespace HRMS.Models.DTOs;

public class CreateLeaveBalanceDto
{
    public string EmpNo { get; set; } = string.Empty;

    public int Annual { get; set; }

    public int Sick { get; set; }

    public int Unpaid { get; set; }
}
