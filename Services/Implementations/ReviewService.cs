using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{   
        public class ReviewService : IReviewService
        {
            private readonly IAppointmentRepository _appointmentRepository;
            private readonly IReviewRepository _reviewRepository;
            private readonly IUnitOfWork _unitOfWork;

            public ReviewService(
                IAppointmentRepository appointmentRepository,
                IReviewRepository reviewRepository,
                IUnitOfWork unitOfWork)
            {
                _appointmentRepository = appointmentRepository;
                _reviewRepository = reviewRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<ReviewResponse> CreateAsync(Guid userId, ReviewRequest request)
            {
                if (request.Rating < 1 || request.Rating > 5)
                {
                    throw new Exception("La calificación debe estar entre 1 y 5.");
                }

                if (request.Comment?.Length > 1000)
                {
                    throw new Exception("El comentario no puede superar los 1000 caracteres.");
                }

                var appointment = await _appointmentRepository.GetWithDetailsByIdAsync(request.AppointmentId);
                if (appointment is null)
                {
                    throw new Exception("No se encontró el turno.");
                }

                if (appointment.ClientId != userId)
                {
                    throw new Exception("No podés reseñar este turno.");
                }

                if (appointment.Status != AppointmentStatus.Completed)
                {
                    throw new Exception("Solo se pueden reseñar turnos finalizados.");
                }

                var existingReview = await _reviewRepository.GetByAppointmentIdAsync(appointment.Id);

                if (existingReview is not null)
                {
                    throw new Exception("Este turno ya tiene una reseña.");
                }

                var review = new Review
                {
                    AppointmentId = appointment.Id,
                    BusinessId = appointment.BusinessId,
                    Rating = request.Rating,
                    Comment = request.Comment?.Trim()
                };

                await _reviewRepository.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                return new ReviewResponse
                {
                    Id = review.Id,
                    Author = $"{appointment.Client.FirstName} {appointment.Client.LastName}",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };
            }

        }
    }
