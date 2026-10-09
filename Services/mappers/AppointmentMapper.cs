using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Entities;

namespace NexusFlow.Services.mappers;

public static class AppointmentMapper
{
    public static AppointmentResponse ToResponse(Appointment appointment)
    {
        return ToResponse(appointment, appointment.Service, appointment.Employee);
    }

    public static AppointmentResponse ToResponse(Appointment appointment, Service service, UserBusiness employee)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            ServiceName = service.Name,
            EmployeeName = $"{employee.User.FirstName} {employee.User.LastName}",
            StartDate = appointment.StartDate,
            EndDate = appointment.EndDate,
            Status = appointment.Status,
            Price = service.Price
        };
    }
}