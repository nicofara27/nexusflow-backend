    public interface IBusinessScheduleRepository
    {
        Task<List<BusinessSchedule>> GetByBusinessIdAsync(Guid businessId);
        void Add(BusinessSchedule businessSchedule);
        void Remove(BusinessSchedule businessSchedule);
    }
