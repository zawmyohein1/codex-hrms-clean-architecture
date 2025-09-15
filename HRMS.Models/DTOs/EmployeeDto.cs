namespace HRMS.Models.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }

    public string EmpNo { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public DateTime HireDate { get; set; }
}
