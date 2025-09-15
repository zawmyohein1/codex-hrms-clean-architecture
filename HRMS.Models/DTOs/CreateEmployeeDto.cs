namespace HRMS.Models.DTOs;

public class CreateEmployeeDto
{
    public string EmpNo { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public DateTime HireDate { get; set; }
}
