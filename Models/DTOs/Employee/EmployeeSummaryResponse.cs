namespace NexusFlow.Models.DTOs.Employee
{
    public class EmployeeSummaryResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
