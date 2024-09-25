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
}

public class Availability : BaseEntity
{
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public DayOfWeek Day { get; set; }
    public List<TimeSlot> TimeSlots { get; set; } = new();

    public bool IsAvailableAt(DateTime dateTime)
    {
        return dateTime.DayOfWeek == Day &&
               TimeSlots.Any(slot => slot.Contains(dateTime.TimeOfDay));
    }
}

public class TimeSlot : BaseEntity
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public bool Contains(TimeSpan time)
    {
        return time >= StartTime && time.Add(TimeSpan.FromMinutes(55)) <= EndTime;
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
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsValid(List<Availability> tutorAvailabilities)
    {
        return tutorAvailabilities.Any(a => a.IsAvailableAt(StartTime));
    }
}