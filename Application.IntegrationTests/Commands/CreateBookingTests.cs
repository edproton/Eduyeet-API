using Application.Features.CreateBooking;
using Application.Features.GetStudentBookings;
using Application.IntegrationTests.Shared;
using Application.Services;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

namespace Application.IntegrationTests.Commands;

[TestClass]
public class BookingIntegrationTests : IntegrationTestBase
{
    private Guid _studentId;
    private Guid _tutorId;
    private Guid _qualificationId;

    protected override async Task SeedTestData()
    {
        var learningSystem = new LearningSystem { Name = "Test Learning System" };
        await LearningSystemRepository.AddAsync(learningSystem, CancellationToken.None);

        var subject = new Subject { Name = "Mathematics", LearningSystem = learningSystem };
        await SubjectRepository.AddAsync(subject, CancellationToken.None);

        var qualification = new Qualification { Name = "Algebra", Subject = subject };
        await QualificationRepository.AddAsync(qualification, CancellationToken.None);

        var tutor = new Tutor
        {
            Name = "John Doe",
            TimeZoneId = "America/New_York",
            AvailableQualifications = new List<Qualification> { qualification },
            Availabilities = new List<Availability>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    TimeSlots = new List<TimeSlot>
                    {
                        new() { StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) }
                    }
                }
            }
        };
        await PersonRepository.AddAsync(tutor, CancellationToken.None);

        var student = new Student
        {
            Name = "Jane Smith",
            TimeZoneId = "Europe/London",
            InterestedQualifications = new List<Qualification> { qualification }
        };
        await PersonRepository.AddAsync(student, CancellationToken.None);

        await UnitOfWork.SaveChangesAsync(CancellationToken.None);

        _studentId = student.Id;
        _tutorId = tutor.Id;
        _qualificationId = qualification.Id;
    }

    [TestMethod]
    public async Task CreateBooking_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var command = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 0, 0) // A Monday at 10 AM
        );

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.StudentId.Should().Be(_studentId);
        response.TutorId.Should().Be(_tutorId);
        response.QualificationId.Should().Be(_qualificationId);
        response.StudentLocalStartTime.Should().Be(new DateTimeOffset(2024, 3, 18, 10, 0, 0, TimeSpan.Zero));
        response.StudentLocalEndTime.Should().Be(new DateTimeOffset(2024, 3, 18, 11, 0, 0, TimeSpan.Zero));
        response.TutorLocalStartTime.Should().Be(new DateTimeOffset(2024, 3, 18, 5, 0, 0, TimeSpan.FromHours(-5)));
        response.TutorLocalEndTime.Should().Be(new DateTimeOffset(2024, 3, 18, 6, 0, 0, TimeSpan.FromHours(-5)));
    }

    [TestMethod]
    public async Task CreateBooking_NonExistentStudent_ShouldReturnError()
    {
        // Arrange
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 0, 0)
        );

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Student.NotFound");
    }

    [TestMethod]
    public async Task CreateBooking_TutorNotAvailable_ShouldReturnError()
    {
        // Arrange
        var command = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 19, 10, 0, 0) // A Tuesday
        );

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Booking.TutorNotAvailable");
    }

    [TestMethod]
    public async Task CreateBooking_OverlappingBooking_ShouldReturnError()
    {
        // Arrange
        var firstBooking = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 0, 0)
        );
        await Mediator.Send(firstBooking);

        var overlappingBooking = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 30, 0)
        );

        // Act
        var result = await Mediator.Send(overlappingBooking);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Booking.OverlappingBooking");
    }

    [TestMethod]
    public async Task GetStudentBookings_ValidRequest_ShouldReturnBookings()
    {
        // Arrange
        var createBookingCommand = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 0, 0)
        );
        await Mediator.Send(createBookingCommand);

        var query = new GetStudentBookingsQuery(_studentId);

        // Act
        var result = await Mediator.Send(query);

        // Assert
        result.IsError.Should().BeFalse();
        var bookings = result.Value;
        bookings.Should().HaveCount(1);
        var booking = bookings.First();
        booking.TutorId.Should().Be(_tutorId);
        booking.QualificationId.Should().Be(_qualificationId);
        booking.StartTime.Should().Be(new DateTimeOffset(2024, 3, 18, 10, 0, 0, TimeSpan.Zero));
        booking.EndTime.Should().Be(new DateTimeOffset(2024, 3, 18, 11, 0, 0, TimeSpan.Zero));
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
        result.FirstError.Code.Should().Be("StudentNotFound");
    }

    [TestMethod]
    public async Task CreateBooking_DifferentCountries_ShouldSucceed()
    {
        // Arrange
        var command = new CreateBookingCommand(
            _studentId,
            _tutorId,
            _qualificationId,
            new DateTime(2024, 3, 18, 10, 0, 0) // A Monday at 10 AM
        );

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        var response = result.Value;
        
        // Verify that the student and tutor are from different countries
        var student = await PersonRepository.GetByIdAsync(_studentId, CancellationToken.None);
        var tutor = await PersonRepository.GetByIdAsync(_tutorId, CancellationToken.None);
        
        student.TimeZoneId.Should().NotBe(tutor.TimeZoneId);
        
        // Verify that the booking times are correct for both student and tutor
        response.StudentLocalStartTime.Should().Be(new DateTimeOffset(2024, 3, 18, 10, 0, 0, TimeSpan.Zero));
        response.TutorLocalStartTime.Should().Be(new DateTimeOffset(2024, 3, 18, 5, 0, 0, TimeSpan.FromHours(-5)));
    }
}