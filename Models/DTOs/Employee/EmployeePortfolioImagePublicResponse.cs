namespace NexusFlow.Models.DTOs.Employee
{
    public class EmployeePortfolioImagePublicResponse
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
