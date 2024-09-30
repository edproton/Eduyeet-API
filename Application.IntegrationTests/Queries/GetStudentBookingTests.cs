using Application.Features.GetStudentBookings;
using Application.IntegrationTests.Shared;
using Application.IntegrationTests.Shared.Builders;
using Application.Services;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

namespace Application.IntegrationTests.Queries;

[TestClass]
public class GetStudentBookingsTests : IntegrationTestBase
{
    private Student _studentInPortugal;
    private Student _studentInLondon;
    private Tutor _tutorInUK;
    private Tutor _tutorInDubai;
    private Qualification _qualification;
    private Booking _bookingUKPortugal;
    private Booking _bookingDubaiLondon;

    protected override async Task SeedTestData()
    {
        // Step 1: Set up learning system, subject, and qualification
        var learningSystem = await new LearningSystemBuilder()
            .WithName("International Learning System")
            .BuildAndSaveAsync(LearningSystemRepository, UnitOfWork);

        var subject = await new SubjectBuilder()
            .WithName("Global Studies")
            .WithLearningSystem(learningSystem)
            .BuildAndSaveAsync(SubjectRepository, UnitOfWork);

        _qualification = await new QualificationBuilder()
            .WithName("International Relations")
            .WithSubject(subject)
            .BuildAndSaveAsync(QualificationRepository, UnitOfWork);

        // Step 2: Create students in different time zones
        _studentInPortugal = await new StudentBuilder()
            .WithName("Maria Silva")
            .WithTimeZoneId("Europe/Lisbon")
            .WithInterestedQualification(_qualification)
            .BuildAndSaveAsync(StudentRepository, UnitOfWork);

        _studentInLondon = await new StudentBuilder()
            .WithName("John Smith")
            .WithTimeZoneId("Europe/London")
            .WithInterestedQualification(_qualification)
            .BuildAndSaveAsync(StudentRepository, UnitOfWork);

        // Step 3: Create tutors in different time zones
        _tutorInUK = await new TutorBuilder()
            .WithName("Emma Brown")
            .WithTimeZoneId("Europe/London")
            .WithQualification(_qualification)
            .BuildAndSaveAsync(TutorRepository, UnitOfWork);

        _tutorInDubai = await new TutorBuilder()
            .WithName("Ahmed Al-Maktoum")
            .WithTimeZoneId("Asia/Dubai")
            .WithQualification(_qualification)
            .BuildAndSaveAsync(TutorRepository, UnitOfWork);

        // Step 4: Set up availabilities for tutors
        await new AvailabilityBuilder()
            .WithDay(DayOfWeek.Monday)
            .WithTimeSlot(new TimeSlotBuilder()
                .WithStartTime(new TimeSpan(9, 0, 0))
                .WithEndTime(new TimeSpan(17, 0, 0))
                .Build())
            .WithTutor(_tutorInUK)
            .BuildAndSaveAsync(AvailabilityRepository, UnitOfWork);

        await new AvailabilityBuilder()
            .WithDay(DayOfWeek.Tuesday)
            .WithTimeSlot(new TimeSlotBuilder()
                .WithStartTime(new TimeSpan(8, 0, 0))
                .WithEndTime(new TimeSpan(16, 0, 0))
                .Build())
            .WithTutor(_tutorInDubai)
            .BuildAndSaveAsync(AvailabilityRepository, UnitOfWork);

        // Step 5: Create bookings
        var utcNow = DateTime.UtcNow;
        _bookingUKPortugal = await new BookingBuilder()
            .WithStudent(_studentInPortugal)
            .WithTutor(_tutorInUK)
            .WithQualification(_qualification)
            .WithStartTime(utcNow.AddDays(1).Date.AddHours(14)) // 2 PM UTC
            .WithEndTime(utcNow.AddDays(1).Date.AddHours(15))   // 3 PM UTC
            .BuildAndSaveAsync(BookingRepository, UnitOfWork);

        _bookingDubaiLondon = await new BookingBuilder()
            .WithStudent(_studentInLondon)
            .WithTutor(_tutorInDubai)
            .WithQualification(_qualification)
            .WithStartTime(utcNow.AddDays(2).Date.AddHours(9))  // 9 AM UTC
            .WithEndTime(utcNow.AddDays(2).Date.AddHours(10))   // 10 AM UTC
            .BuildAndSaveAsync(BookingRepository, UnitOfWork);
    }

    [TestMethod]
    public async Task GetStudentBookings_UKTutorPortugalStudent_ShouldReturnCorrectLocalTimes()
    {
        // Arrange
        var query = new GetStudentBookingsQuery(_studentInPortugal.Id);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        result.IsError.Should().BeFalse();
        var bookings = result.Value.ToList();
        bookings.Should().HaveCount(1);

        var booking = bookings[0];
        booking.Id.Should().Be(_bookingUKPortugal.Id);
        booking.TutorId.Should().Be(_tutorInUK.Id);
        booking.TutorName.Should().Be(_tutorInUK.Name);
        booking.QualificationId.Should().Be(_qualification.Id);
        booking.QualificationName.Should().Be(_qualification.Name);

        // Verify local time for Portugal (UTC+0/+1)
        var timeZoneService = GetService<TimeZoneService>();
        var localStartTime = timeZoneService.ConvertFromUtc(_bookingUKPortugal.StartTime, _studentInPortugal.TimeZoneId);
        var localEndTime = timeZoneService.ConvertFromUtc(_bookingUKPortugal.EndTime, _studentInPortugal.TimeZoneId);

        booking.StartTime.Should().Be(localStartTime);
        booking.EndTime.Should().Be(localEndTime);
    }

    [TestMethod]
    public async Task GetStudentBookings_DubaiTutorLondonStudent_ShouldReturnCorrectLocalTimes()
    {
        // Arrange
        var query = new GetStudentBookingsQuery(_studentInLondon.Id);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        result.IsError.Should().BeFalse();
        var bookings = result.Value.ToList();
        bookings.Should().HaveCount(1);

        var booking = bookings[0];
        booking.Id.Should().Be(_bookingDubaiLondon.Id);
        booking.TutorId.Should().Be(_tutorInDubai.Id);
        booking.TutorName.Should().Be(_tutorInDubai.Name);
        booking.QualificationId.Should().Be(_qualification.Id);
        booking.QualificationName.Should().Be(_qualification.Name);

        // Verify local time for London (UTC+0/+1)
        var timeZoneService = GetService<TimeZoneService>();
        var localStartTime = timeZoneService.ConvertFromUtc(_bookingDubaiLondon.StartTime, _studentInLondon.TimeZoneId);
        var localEndTime = timeZoneService.ConvertFromUtc(_bookingDubaiLondon.EndTime, _studentInLondon.TimeZoneId);

        booking.StartTime.Should().Be(localStartTime);
        booking.EndTime.Should().Be(localEndTime);
    }

    [TestMethod]
    public async Task GetStudentBookings_NonExistentStudent_ShouldReturnError()
    {
        // Arrange
        var query = new GetStudentBookingsQuery(Guid.NewGuid());

        // Act
        var result = await Mediator.Send(query);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Contain("student with the ID");
    }

    [TestMethod]
    public async Task GetStudentBookings_StudentWithNoBookings_ShouldReturnEmptyList()
    {
        // Arrange
        var studentWithNoBookings = await new StudentBuilder()
            .WithName("No Bookings Student")
            .WithTimeZoneId("Europe/Berlin")
            .BuildAndSaveAsync(StudentRepository, UnitOfWork);

        var query = new GetStudentBookingsQuery(studentWithNoBookings.Id);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}