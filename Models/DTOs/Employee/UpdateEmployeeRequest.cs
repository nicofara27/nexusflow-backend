using NexusFlow.Models.Entities;

namespace NexusFlow.Models.DTOs.Employee
{
    public class UpdateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<Guid> ServiceIds { get; set; } = [];
    }
}
