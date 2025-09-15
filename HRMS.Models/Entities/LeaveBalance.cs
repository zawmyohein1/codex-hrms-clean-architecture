namespace HRMS.Models.Entities;

public class LeaveBalance
{
    public int Id { get; set; }

    public string EmpNo { get; set; } = string.Empty;

    public int Annual { get; set; }

    public int Sick { get; set; }

    public int Unpaid { get; set; }
}
