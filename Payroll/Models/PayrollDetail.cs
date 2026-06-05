namespace Payroll.Models
{
    public class PayrollDetail
    {
        public int DetailId { get; init; }
        public int RunId { get; init; }
        public int EmployeeId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string EmployeeCode { get; init; } = string.Empty;
        public string DepartmentName { get; init; } = string.Empty;
        public decimal BasicSalary { get; init; }
        public int WorkingDays { get; init; }
        public int DaysPresent { get; init; }
        public decimal GrossPay { get; init; }
        public decimal PFDeduction { get; init; }
        public decimal ProfessionalTax { get; init; }
        public decimal NetPay { get; init; }
    }
}
