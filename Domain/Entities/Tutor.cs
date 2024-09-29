using Domain.Entities.Shared;
using Domain.Enums;

namespace Domain.Entities;

public class Tutor() : Person(PersonTypeEnum.Tutor)
{
    public List<Qualification> AvailableQualifications { get; set; } = [];

    public List<Availability> Availabilities { get; set; } = [];

    public List<Booking> Bookings { get; set; } = [];
}

public class Student() : Person(PersonTypeEnum.Student)
{
    public List<Booking> Bookings { get; set; } = [];
    public List<Qualification> InterestedQualifications { get; set; } = [];
}

public class Availability : BaseEntity
{
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public DayOfWeek Day { get; set; }
    public List<TimeSlot> TimeSlots { get; set; } = new();

    public bool IsAvailableAt(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime must be in UTC", nameof(utcDateTime));
        }

        if (utcDateTime.DayOfWeek != Day)
        {
            return false;
        }

        var timeOfDay = utcDateTime.TimeOfDay;
        return TimeSlots.Any(slot => slot.Contains(timeOfDay));
    }
}

public class TimeSlot : BaseEntity
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public bool Contains(TimeSpan time)
    {
        if (EndTime > StartTime)
        {
            return time >= StartTime && time < EndTime;
        }
    
        return time >= StartTime || time < EndTime;
    }

    public bool ContainsRange(TimeSpan start, TimeSpan end)
    {
        return Contains(start) && (Contains(end) || end == TimeSpan.FromHours(24));
    }
}

public class Booking : BaseEntity
{
    public Guid StudentId { get; set; }
    public required Student Student { get; set; }
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public Guid QualificationId { get; set; }
    public required Qualification Qualification { get; set; }

    private DateTime _startTime;
    public DateTime StartTime
    {
        get => _startTime;
        set => _startTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private DateTime _endTime;
    public DateTime EndTime
    {
        get => _endTime;
        set => _endTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}