namespace Payroll.Models
{
    public class Employee
    {
        public int EmployeeId { get; init; }
        public string EmployeeCode { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public decimal BasicSalary { get; init; }
        public int DepartmentId { get; init; }
        public string DepartmentName { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime JoiningDate { get; init; }
    }
}
