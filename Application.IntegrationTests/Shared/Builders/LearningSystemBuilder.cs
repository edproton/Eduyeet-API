using Application.Repositories;
using Application.Repositories.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.IntegrationTests.Shared.Builders;

public class LearningSystemBuilder
{
    private readonly LearningSystem _learningSystem = new();

    public LearningSystemBuilder WithName(string name)
    {
        _learningSystem.Name = name;
        return this;
    }

    public LearningSystem Build() => _learningSystem;

    public async Task<LearningSystem> BuildAndSaveAsync(
        ILearningSystemRepository learningSystemRepository,
        IUnitOfWork unitOfWork)
    {
        var learningSystem = Build();
        await learningSystemRepository.AddAsync(learningSystem, default);
        await unitOfWork.SaveChangesAsync();
        return learningSystem;
    }
}

public class SubjectBuilder
{
    private readonly Subject _subject = new();

    public SubjectBuilder WithName(string name)
    {
        _subject.Name = name;
        return this;
    }

    public SubjectBuilder WithLearningSystem(LearningSystem learningSystem)
    {
        _subject.LearningSystem = learningSystem;
        _subject.LearningSystemId = learningSystem.Id;
        return this;
    }

    public Subject Build() => _subject;

    public async Task<Subject> BuildAndSaveAsync(ISubjectRepository subjectRepository, IUnitOfWork unitOfWork)
    {
        var subject = Build();
        await subjectRepository.AddAsync(subject, default);
        await unitOfWork.SaveChangesAsync();
        return subject;
    }
}

public class QualificationBuilder
{
    private readonly Qualification _qualification = new();

    public QualificationBuilder WithName(string name)
    {
        _qualification.Name = name;
        return this;
    }

    public QualificationBuilder WithSubject(Subject subject)
    {
        _qualification.Subject = subject;
        _qualification.SubjectId = subject.Id;
        return this;
    }

    public Qualification Build() => _qualification;

    public async Task<Qualification> BuildAndSaveAsync(
        IQualificationRepository qualificationRepository,
        IUnitOfWork unitOfWork)
    {
        var qualification = Build();
        await qualificationRepository.AddAsync(qualification, default);
        await unitOfWork.SaveChangesAsync();
        return qualification;
    }
}

public abstract class PersonBuilder<T, TSelf>
    where T : Person, new()
    where TSelf : PersonBuilder<T, TSelf>
{
    protected readonly T _person = new();

    public TSelf WithName(string name)
    {
        _person.Name = name;
        return (TSelf)this;
    }

    public TSelf WithTimeZoneId(string timeZoneId)
    {
        _person.TimeZoneId = timeZoneId;
        return (TSelf)this;
    }

    public abstract T Build();
}

public class TutorBuilder : PersonBuilder<Tutor, TutorBuilder>
{
    public TutorBuilder WithQualification(Qualification qualification)
    {
        _person.AvailableQualifications.Add(qualification);
        return this;
    }

    public TutorBuilder WithAvailability(Availability availability)
    {
        _person.Availabilities.Add(availability);
        return this;
    }

    public override Tutor Build() => _person;

    public async Task<Tutor> BuildAndSaveAsync(ITutorRepository tutorRepository, IUnitOfWork unitOfWork)
    {
        var tutor = Build();
        await tutorRepository.AddAsync(tutor, default);
        await unitOfWork.SaveChangesAsync();
        return tutor;
    }
}

public class StudentBuilder : PersonBuilder<Student, StudentBuilder>
{
    public StudentBuilder WithInterestedQualification(Qualification qualification)
    {
        _person.InterestedQualifications.Add(qualification);
        return this;
    }

    public override Student Build() => _person;

    public async Task<Student> BuildAndSaveAsync(IStudentRepository studentRepository, IUnitOfWork unitOfWork)
    {
        var student = Build();
        await studentRepository.AddAsync(student, default);
        await unitOfWork.SaveChangesAsync();
        return student;
    }
}

public class AvailabilityBuilder
{
    private readonly Availability _availability = new();

    public AvailabilityBuilder WithDay(DayOfWeek day)
    {
        _availability.Day = day;
        return this;
    }

    public AvailabilityBuilder WithTimeSlot(TimeSlot timeSlot)
    {
        _availability.TimeSlots.Add(timeSlot);
        return this;
    }

    public AvailabilityBuilder WithTutor(Tutor tutor)
    {
        _availability.Tutor = tutor;
        _availability.TutorId = tutor.Id;
        return this;
    }

    public Availability Build() => _availability;

    public async Task BuildAndSaveAsync(IAvailabilityRepository availabilityRepository, IUnitOfWork unitOfWork)
    {
        var availability = Build();
        await availabilityRepository.AddAsync(availability, default);
        await unitOfWork.SaveChangesAsync();
    }
}

public class TimeSlotBuilder
{
    private readonly TimeSlot _timeSlot = new();

    public TimeSlotBuilder WithStartTime(TimeSpan startTime)
    {
        _timeSlot.StartTime = startTime;
        return this;
    }

    public TimeSlotBuilder WithEndTime(TimeSpan endTime)
    {
        _timeSlot.EndTime = endTime;
        return this;
    }

    public TimeSlot Build() => _timeSlot;
}

public class BookingBuilder
{
    private readonly Booking _booking = new();

    public BookingBuilder WithStudent(Student student)
    {
        _booking.Student = student;
        _booking.StudentId = student.Id;
        return this;
    }

    public BookingBuilder WithTutor(Tutor tutor)
    {
        _booking.Tutor = tutor;
        _booking.TutorId = tutor.Id;
        return this;
    }

    public BookingBuilder WithQualification(Qualification qualification)
    {
        _booking.Qualification = qualification;
        _booking.QualificationId = qualification.Id;
        return this;
    }

    public BookingBuilder WithStartTime(DateTime startTime)
    {
        _booking.StartTime = startTime.ToUniversalTime();
        return this;
    }

    public BookingBuilder WithEndTime(DateTime endTime)
    {
        _booking.EndTime = endTime.ToUniversalTime();
        return this;
    }

    public Booking Build() => _booking;

    public async Task<Booking> BuildAndSaveAsync(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        var booking = Build();
        await bookingRepository.AddAsync(booking, default);
        await unitOfWork.SaveChangesAsync();
        return booking;
    }
}