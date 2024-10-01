using Application.Features.SetTutorAvailability;
using Application.IntegrationTests.Shared;
using Application.Services;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

namespace Application.IntegrationTests.Commands;

[TestClass]
public class SetTutorAvailabilityTests : IntegrationTestBase
{
    private Guid _tutorId;
    private Guid _qualificationId;
    private Guid _subjectId;
    private Guid _learningSystemId;
    private TimeZoneService _timeZoneService;

    protected override async Task SeedTestData()
    {
        // Create a test learning system, subject, and qualification
        var learningSystem = new LearningSystem { Name = "Test Learning System" };
        await LearningSystemRepository.AddAsync(learningSystem, CancellationToken.None);

        var subject = new Subject { Name = "Mathematics", LearningSystem = learningSystem };
        await SubjectRepository.AddAsync(subject, CancellationToken.None);

        var qualification = new Qualification { Name = "Algebra", Subject = subject };
        await QualificationRepository.AddAsync(qualification, CancellationToken.None);

        // Create a test tutor with the qualification
        var tutor = new Tutor
        {
            Name = "John",
            TimeZoneId = "America/New_York",
            AvailableQualifications = new List<Qualification> { qualification }
        };
        await PersonRepository.AddAsync(tutor, CancellationToken.None);

        await UnitOfWork.SaveChangesAsync(CancellationToken.None);

        _tutorId = tutor.Id;
        _qualificationId = qualification.Id;
        _subjectId = subject.Id;
        _learningSystemId = learningSystem.Id;

        // Initialize TimeZoneService
        _timeZoneService = GetService<TimeZoneService>();
    }

    [TestMethod]
    public async Task Handle_UpdateExistingAvailability_ShouldSucceed()
    {
        // Arrange
        var initialCommand = new SetTutorAvailabilityCommand(
            _tutorId,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
                })
            });
        await Mediator.Send(initialCommand);

        var updateCommand = new SetTutorAvailabilityCommand(
            _tutorId,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0))
                })
            });

        // Act
        var result = await Mediator.Send(updateCommand);

        // Assert
        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.TutorId.Should().Be(_tutorId);
        response.Availabilities.Should().HaveCount(1);
        response.Availabilities[0].Day.Should().Be(DayOfWeek.Monday);
        response.Availabilities[0].TimeSlots.Should().HaveCount(1);
        response.Availabilities[0].TimeSlots[0].StartTime.Should().Be(new TimeSpan(10, 0, 0));
        response.Availabilities[0].TimeSlots[0].EndTime.Should().Be(new TimeSpan(18, 0, 0));

        // Verify the updated availability was correctly stored in the database
        var storedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, CancellationToken.None);
        storedTutor.Should().NotBeNull();
        storedTutor.Availabilities.Should().HaveCount(1);
        storedTutor.Availabilities[0].Day.Should().Be(DayOfWeek.Monday);
        storedTutor.Availabilities[0].TimeSlots.Should().HaveCount(1);

        // Convert the expected local times to UTC using TimeZoneService
        var expectedStartTimeUtc = _timeZoneService.ConvertTimeToUtc(new TimeSpan(10, 0, 0), storedTutor.TimeZoneId, DayOfWeek.Monday);
        var expectedEndTimeUtc = _timeZoneService.ConvertTimeToUtc(new TimeSpan(18, 0, 0), storedTutor.TimeZoneId, DayOfWeek.Monday);

        // Assert the stored UTC times match our expectations
        storedTutor.Availabilities[0].TimeSlots[0].StartTime.Should().Be(expectedStartTimeUtc);
        storedTutor.Availabilities[0].TimeSlots[0].EndTime.Should().Be(expectedEndTimeUtc);

        // Convert the stored UTC times back to local time and verify
        var localStartTime = _timeZoneService.ConvertTimeFromUtc(storedTutor.Availabilities[0].TimeSlots[0].StartTime, storedTutor.TimeZoneId, DayOfWeek.Monday);
        var localEndTime = _timeZoneService.ConvertTimeFromUtc(storedTutor.Availabilities[0].TimeSlots[0].EndTime, storedTutor.TimeZoneId, DayOfWeek.Monday);

        localStartTime.Should().Be(new TimeSpan(10, 0, 0));
        localEndTime.Should().Be(new TimeSpan(18, 0, 0));
    }
    
      [TestMethod]
    public async Task Handle_NonExistentTutor_ShouldReturnError()
    {
        // Arrange
        var command = new SetTutorAvailabilityCommand(
            Guid.NewGuid(),
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
                })
            });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Tutor.NotFound");
    }

    [TestMethod]
    public async Task Handle_TutorWithNoQualifications_ShouldReturnError()
    {
        // Arrange
        var tutorWithNoQualifications = new Tutor
        {
            Name = "Jane",
            TimeZoneId = "Europe/London"
        };
        await PersonRepository.AddAsync(tutorWithNoQualifications, CancellationToken.None);
        await UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var command = new SetTutorAvailabilityCommand(
            tutorWithNoQualifications.Id,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
                })
            });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("NoQualifications");
    }

    [TestMethod]
    public async Task Handle_MultipleAvailabilities_ShouldSucceed()
    {
        // Arrange
        var command = new SetTutorAvailabilityCommand(
            _tutorId,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
                    new(new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0))
                }),
                new(DayOfWeek.Wednesday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0))
                })
            });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.Availabilities.Should().HaveCount(2);
        
        var mondayAvailability = response.Availabilities.First(a => a.Day == DayOfWeek.Monday);
        mondayAvailability.TimeSlots.Should().HaveCount(2);
        
        var wednesdayAvailability = response.Availabilities.First(a => a.Day == DayOfWeek.Wednesday);
        wednesdayAvailability.TimeSlots.Should().HaveCount(1);

        // Verify database storage
        var storedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, CancellationToken.None);
        storedTutor.Availabilities.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Handle_OverlappingTimeSlots_ShouldSucceed()
    {
        // Arrange
        var command = new SetTutorAvailabilityCommand(
            _tutorId,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0)),
                    new(new TimeSpan(12, 0, 0), new TimeSpan(17, 0, 0))
                })
            });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.Availabilities.Should().HaveCount(1);
        response.Availabilities[0].TimeSlots.Should().HaveCount(2);

        // Verify database storage
        var storedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, CancellationToken.None);
        storedTutor.Availabilities.Should().HaveCount(1);
        storedTutor.Availabilities[0].TimeSlots.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task Handle_MidnightCrossing_ShouldSucceed()
    {
        // Arrange
        var command = new SetTutorAvailabilityCommand(
            _tutorId,
            new List<AvailabilityDto>
            {
                new(DayOfWeek.Monday, new List<TimeSlotDto>
                {
                    new(new TimeSpan(22, 0, 0), new TimeSpan(26, 0, 0))  // Use 26:00:00 to represent 2:00 AM the next day
                })
            });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse($"Error occurred: {result.Errors.First().Description}");
        var response = result.Value;
        response.Availabilities.Should().HaveCount(1);
        response.Availabilities[0].TimeSlots.Should().HaveCount(1);

        // Verify database storage and UTC conversion
        var storedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, CancellationToken.None);
        storedTutor.Availabilities.Should().HaveCount(1);
        storedTutor.Availabilities[0].TimeSlots.Should().HaveCount(1);

        var startTimeUtc = _timeZoneService.ConvertTimeToUtc(new TimeSpan(22, 0, 0), storedTutor.TimeZoneId, DayOfWeek.Monday);
        var endTimeUtc = _timeZoneService.ConvertTimeToUtc(new TimeSpan(26, 0, 0), storedTutor.TimeZoneId, DayOfWeek.Monday);

        storedTutor.Availabilities[0].TimeSlots[0].StartTime.Should().Be(startTimeUtc);
        storedTutor.Availabilities[0].TimeSlots[0].EndTime.Should().Be(endTimeUtc);
    }
}