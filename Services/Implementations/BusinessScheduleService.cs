using NexusFlow.Models.DTOs.BusinessSchedule;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;

public class BusinessScheduleService : IBusinessScheduleService
{
    private readonly IBusinessScheduleRepository _businessScheduleRepository;
    private readonly IUserBusinessRepository _userBusinessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BusinessScheduleService(
        IBusinessScheduleRepository businessScheduleRepository,
        IUserBusinessRepository userBusinessRepository,
        IUnitOfWork unitOfWork)
    {
        _businessScheduleRepository = businessScheduleRepository;
        _userBusinessRepository = userBusinessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BusinessScheduleResponse>> GetAsync(Guid userId)
    {
        var admin = await GetAdminAsync(userId);

        var schedules = await _businessScheduleRepository
            .GetByBusinessIdAsync(admin.BusinessId);

        return schedules
            .OrderBy(s => GetDayOrder(s.DayOfWeek))
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<BusinessScheduleResponse>> UpdateAsync(
        Guid userId,
        BusinessScheduleRequest request)
    {
        ValidateRequest(request);

        var admin = await GetAdminAsync(userId);

        var existingSchedules = await _businessScheduleRepository
            .GetByBusinessIdAsync(admin.BusinessId);

        foreach (var schedule in existingSchedules)
        {
            var requestedDay = request.Days
                .FirstOrDefault(d => d.DayOfWeek == schedule.DayOfWeek);

            if (requestedDay is null)
            {
                _businessScheduleRepository.Remove(schedule);
                continue;
            }

            schedule.StartTime = requestedDay.StartTime;
            schedule.EndTime = requestedDay.EndTime;
        }

        var existingDays = existingSchedules
            .Select(s => s.DayOfWeek)
            .ToHashSet();

        foreach (var day in request.Days)
        {
            if (existingDays.Contains(day.DayOfWeek))
            {
                continue;
            }

            _businessScheduleRepository.Add(new BusinessSchedule
            {
                BusinessId = admin.BusinessId,
                DayOfWeek = day.DayOfWeek,
                StartTime = day.StartTime,
                EndTime = day.EndTime
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedSchedules = await _businessScheduleRepository
            .GetByBusinessIdAsync(admin.BusinessId);

        return updatedSchedules
            .OrderBy(s => GetDayOrder(s.DayOfWeek))
            .Select(MapToResponse)
            .ToList();
    }

    private async Task<UserBusiness> GetAdminAsync(Guid userId)
    {
        var admin = await _userBusinessRepository
            .GetByUserIdAsync(userId, BusinessRole.Admin);

        if (admin is null)
        {
            throw new UnauthorizedAccessException(
                "El usuario no administra ningún negocio.");
        }

        return admin;
    }

    private static void ValidateRequest(BusinessScheduleRequest request)
    {
        if (request.Days.Count > 7)
        {
            throw new ArgumentException(
                "No se pueden configurar más de siete días.");
        }

        if (request.Days.Any(d => !Enum.IsDefined(d.DayOfWeek)))
        {
            throw new ArgumentException(
                "Uno de los días enviados no es válido.");
        }

        if (request.Days
            .GroupBy(d => d.DayOfWeek)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "No se puede configurar el mismo día más de una vez.");
        }

        if (request.Days.Any(d => d.StartTime >= d.EndTime))
        {
            throw new ArgumentException(
                "El horario de apertura debe ser anterior al horario de cierre.");
        }
    }

    private static BusinessScheduleResponse MapToResponse(
        BusinessSchedule schedule)
    {
        return new BusinessScheduleResponse
        {
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime
        };
    }

    private static int GetDayOrder(DayOfWeek day)
    {
        return day == DayOfWeek.Sunday ? 7 : (int)day;
    }
}