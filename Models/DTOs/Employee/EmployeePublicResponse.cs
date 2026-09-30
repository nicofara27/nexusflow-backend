using NexusFlow.Models.DTOs.Employee;

public class EmployeePublicResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<EmployeePortfolioImagePublicResponse> PortfolioImages { get; set; } = [];
}