namespace NexusFlow.Models.Entities
{
    public class EmployeePortfolioImage
    {
        public Guid Id { get; set; }
        public Guid UserBusinessId { get; set; }
        public UserBusiness UserBusiness { get; set; } = null!;
        public string StorageKey { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
