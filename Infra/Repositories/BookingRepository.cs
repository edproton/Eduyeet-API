using Application.Repositories;
using Domain.Entities;
using Infra.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class BookingRepository(ApplicationDbContext context)
    : Repository<Booking>(context), IBookingRepository
{
    public Task<Booking?> GetOverlappingBookingAsync(
        Guid tutorId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        return Context.Bookings
            .FirstOrDefaultAsync(b =>
                    b.TutorId == tutorId &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime,
                cancellationToken);
    }
}