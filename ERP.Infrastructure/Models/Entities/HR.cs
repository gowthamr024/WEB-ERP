namespace ERP.Infrastructure.Models.Entities
{
    public class EmployeeMaster
    {
        public int EmplId { get; set; }
        public required string EmployeeName { get; set; }
        public int EmployeeAge { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateOfJoin { get; set; }
        public DateTime DateOfResign { get; set; }
        public int DepartmentID { get; set; }
        public int DesignationID { get; set; }
        public int CompanyID { get; set; }
        public required string Paymode { get; set; }
        public List<EmployeeBank> employeeBanks { get; set; } = new();
    }

    public class EmployeeBank
    {
        public int BankID { get; set; }
        public required string BankName { get; set; }
        public string AccountType { get; set; } = "Salary";
        public required int AccountNo { get; set; }
        public required string IFSC { get; set; }
        public string? BankAddress { get; set; }
    }
}
