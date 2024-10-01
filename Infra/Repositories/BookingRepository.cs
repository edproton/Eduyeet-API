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
        var utcStartTime = startTime.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(startTime, DateTimeKind.Utc)
            : startTime.ToUniversalTime();
        
        var utcEndTime = endTime.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(endTime, DateTimeKind.Utc)
            : endTime.ToUniversalTime();

        return Context.Bookings
            .FirstOrDefaultAsync(b =>
                    b.TutorId == tutorId &&
                    b.StartTime < utcEndTime &&
                    b.EndTime > utcStartTime,
                cancellationToken);
    }

    public async Task<List<Booking>> GetBookingsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken)
    {
        return await Context.Bookings
            .Where(b => b.StudentId == studentId)
            .Include(b => b.Tutor)
            .Include(b => b.Qualification)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Booking>> GetBookingsByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken)
    {
        return await Context.Bookings
            .Where(b => b.TutorId == tutorId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Booking>> GetBookingsForTutorInRangeAsync(
        Guid tutorId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        return await Context.Bookings
            .Where(b =>
                b.TutorId == tutorId &&
                b.StartTime < endTime &&
                b.EndTime > startTime)
            .OrderBy(b => b.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Booking>> GetBookingsByStudentIdWithTutorAndQualificationAsync(Guid studentId, CancellationToken cancellationToken)
    {
        return await Context.Bookings
            .Where(b => b.StudentId == studentId)
            .Include(b => b.Tutor) // Eagerly load the Tutor entity
            .Include(b => b.Qualification) // Eagerly load the Qualification entity
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Booking>> GetOverlappingBookingsAsync(
        Guid tutorId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        // Convert dates to DateTimeOffset to match the entity property type
        var startDateOffset = new DateTimeOffset(startDate.Date, TimeSpan.Zero);
        var endDateOffset = new DateTimeOffset(endDate.Date.AddDays(1), TimeSpan.Zero);

        return await Context.Bookings
            .Where(b => b.TutorId == tutorId &&
                        b.StartTime < endDateOffset &&
                        b.EndTime > startDateOffset)
            .ToListAsync(cancellationToken);
    }
}