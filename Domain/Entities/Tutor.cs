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
    public Tutor Tutor { get; set; } = default!;
    public DayOfWeek Day { get; set; }
    public List<TimeSlot> TimeSlots { get; set; } = [];
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
    public Student Student { get; set; } = default!;
    public Guid TutorId { get; set; }
    public Tutor Tutor { get; set; } = default!;
    public Guid QualificationId { get; set; }
    public Qualification Qualification { get; set; } = default!;

    private DateTimeOffset _startTime;
    public DateTimeOffset StartTime
    {
        get => _startTime;
        set => _startTime = value.ToUniversalTime();
    }

    private DateTimeOffset _endTime;
    public DateTimeOffset EndTime
    {
        get => _endTime;
        set => _endTime = value.ToUniversalTime();
    }

    public void SetTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start time must be before end time");
        }

        StartTime = start;
        EndTime = end;
    }
}