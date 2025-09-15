namespace HRMS.Models.Entities;

public class Employee
{
    public int Id { get; set; }

    public string EmpNo { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;
}
