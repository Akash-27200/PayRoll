namespace Payroll.Models
{
    public class PayrollRun
    {
        public int RunId { get; init; }
        public int Month { get; init; }
        public int Year { get; init; }
        public DateTime RunDate { get; init; }
        public bool IsFinalized { get; init; }
        public int EmployeeCount { get; init; }
        public decimal TotalNetPay { get; init; }
        public List<PayrollDetail> Details { get; set; } = new();
    }
}
