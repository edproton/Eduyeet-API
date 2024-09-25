using Domain.Entities.Shared;
using Domain.Enums;

namespace Domain.Entities;

public class Tutor() : Person(PersonTypeEnum.Tutor);

public class TutorQualification : BaseEntity
{
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public Guid QualificationId { get; set; }
    public required Qualification Qualification { get; set; }
}

public class Availability : BaseEntity
{
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public bool IsAvailableAt(DateTime dateTime)
    {
        return dateTime.DayOfWeek == Day &&
               dateTime.TimeOfDay >= StartTime &&
               dateTime.TimeOfDay.Add(TimeSpan.FromMinutes(55)) <= EndTime;
    }
}

public class Booking : BaseEntity
{
    public Guid StudentId { get; set; }
    public required Student Student { get; set; }
    public Guid TutorId { get; set; }
    public required Tutor Tutor { get; set; }
    public Guid SubjectId { get; set; }
    public required Subject Subject { get; set; }
    public DateTime StartTime { get; set; }

    public DateTime EndTime => StartTime.AddMinutes(55);

    public bool IsValid(List<Availability> tutorAvailabilities)
    {
        return tutorAvailabilities.Any(a => a.IsAvailableAt(StartTime));
    }
}