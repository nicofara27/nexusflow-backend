namespace NexusFlow.Models.DTOs.Employee
{
    public class EmployeeSimpleResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
