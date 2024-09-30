using Application.Features.SetTutorAvailability;
using Application.IntegrationTests.Shared;
using Application.IntegrationTests.Shared.Builders;
using Application.Services;
using Domain.Entities;
using FluentAssertions;


namespace Application.IntegrationTests.Commands
{
    [TestClass]
    public class SetTutorAvailabilityTests : IntegrationTestBase
    {
        private Guid _tutorId;
        private LearningSystem _learningSystem = default!;
        private Subject _subject = default!;
        private Qualification _qualification = default!;
        private TimeZoneService _timeZoneService = default!;

        protected override async Task SeedTestData()
        {
            _timeZoneService = GetService<TimeZoneService>();
            _learningSystem = new LearningSystemBuilder().Build();
            await LearningSystemRepository.AddAsync(_learningSystem, default);

            _subject = new SubjectBuilder()
                .WithLearningSystem(_learningSystem)
                .Build();
            await SubjectRepository.AddAsync(_subject, default);

            _qualification = new QualificationBuilder()
                .WithSubject(_subject)
                .Build();
            await QualificationRepository.AddAsync(_qualification, default);

            var tutor = new TutorBuilder()
                .WithName("Dr. Paulo Costa")
                .WithTimeZoneId("Europe/Lisbon")
                .WithQualification(_qualification)
                .Build();

            await TutorRepository.AddAsync(tutor, default);
            await UnitOfWork.SaveChangesAsync();
            _tutorId = tutor.Id;
        }

      [TestMethod]
        [DataRow("Europe/Lisbon", DayOfWeek.Monday, 2)]
        [DataRow("America/New_York", DayOfWeek.Wednesday, 1)] 
        [DataRow("Asia/Tokyo", DayOfWeek.Friday, 3)]
        public async Task SetTutorAvailability_ShouldCreateNewAvailability(string timeZoneId, DayOfWeek dayOfWeek, int slotCount)
        {
            // Arrange
            var learningSystem = new LearningSystemBuilder().Build();
            await LearningSystemRepository.AddAsync(learningSystem, default);

            var subject = new SubjectBuilder()
                .WithLearningSystem(learningSystem)
                .Build();
            await SubjectRepository.AddAsync(subject, default);

            var qualification = new QualificationBuilder()
                .WithSubject(subject)
                .Build();
            await QualificationRepository.AddAsync(qualification, default);

            var tutor = new TutorBuilder()
                .WithName("Dr. Paulo Costa")
                .WithTimeZoneId(timeZoneId)
                .WithQualification(qualification)
                .Build();
            await TutorRepository.AddAsync(tutor, default);
            await UnitOfWork.SaveChangesAsync();

            var availabilities = GenerateAvailabilities(dayOfWeek, slotCount);
            var command = new SetTutorAvailabilityCommand(tutor.Id, availabilities);

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.IsError.Should().BeFalse();

            var updatedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(tutor.Id, default);
            updatedTutor.Should().NotBeNull();
            updatedTutor!.Availabilities.Should().HaveCount(availabilities.Count);

            foreach (var availability in updatedTutor.Availabilities)
            {
                var expectedAvailability = availabilities.Find(a => a.Day == availability.Day);
                expectedAvailability.Should().NotBeNull();

                availability.TimeSlots.Should().HaveCount(expectedAvailability!.TimeSlots.Count);

                for (int i = 0; i < availability.TimeSlots.Count; i++)
                {
                    var actualSlot = availability.TimeSlots[i];
                    var expectedSlot = expectedAvailability.TimeSlots[i];

                    // Assert UTC time slots
                    var utcStartTime = _timeZoneService.ConvertTimeToUtc(expectedSlot.StartTime, timeZoneId, availability.Day);
                    var utcEndTime = _timeZoneService.ConvertTimeToUtc(expectedSlot.EndTime, timeZoneId, availability.Day);

                    actualSlot.StartTime.Should().Be(utcStartTime);
                    actualSlot.EndTime.Should().Be(utcEndTime);

                    // Assert local time conversion
                    var localStartTime = _timeZoneService.ConvertTimeFromUtc(actualSlot.StartTime, timeZoneId, availability.Day);
                    var localEndTime = _timeZoneService.ConvertTimeFromUtc(actualSlot.EndTime, timeZoneId, availability.Day);

                    localStartTime.Should().Be(expectedSlot.StartTime);
                    localEndTime.Should().Be(expectedSlot.EndTime);
                }
            }
        }

        [TestMethod]
        public async Task SetTutorAvailability_ShouldUpdateExistingAvailability()
        {
            // Arrange
            var initialAvailabilities = GenerateAvailabilities(DayOfWeek.Monday, 2);
            var initialCommand = new SetTutorAvailabilityCommand(_tutorId, initialAvailabilities);
            await Mediator.Send(initialCommand);

            var updatedAvailabilities = GenerateAvailabilities(DayOfWeek.Monday, 3); // Same day, different slots
            var updateCommand = new SetTutorAvailabilityCommand(_tutorId, updatedAvailabilities);

            // Act
            var result = await Mediator.Send(updateCommand);

            // Assert
            result.IsError.Should().BeFalse();

            var updatedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, default);
            updatedTutor.Should().NotBeNull();
            updatedTutor!.Availabilities.Should().HaveCount(1);
            updatedTutor.Availabilities[0].TimeSlots.Should().HaveCount(3);
        }

        [TestMethod]
        public async Task SetTutorAvailability_ShouldFailForNonExistentTutor()
        {
            // Arrange
            var nonExistentTutorId = Guid.NewGuid();
            var availabilities = GenerateAvailabilities(DayOfWeek.Monday, 2);
            var command = new SetTutorAvailabilityCommand(nonExistentTutorId, availabilities);

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tutor.NotFound(nonExistentTutorId));
        }

        [TestMethod]
        public async Task SetTutorAvailability_ShouldFailForTutorWithNoQualifications()
        {
            // Arrange
            var tutorWithNoQualifications = new TutorBuilder()
                .WithName("No Qualifications Tutor")
                .WithTimeZoneId("Europe/Lisbon")
                .Build();

            await TutorRepository.AddAsync(tutorWithNoQualifications, default);
            await UnitOfWork.SaveChangesAsync();

            var availabilities = GenerateAvailabilities(DayOfWeek.Friday, 2);
            var command = new SetTutorAvailabilityCommand(tutorWithNoQualifications.Id, availabilities);

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tutor.NoQualifications);
        }

        private List<AvailabilityDto> GenerateAvailabilities(DayOfWeek dayOfWeek, int slotCount)
        {
            var availabilities = new List<AvailabilityDto>();
            var slots = new List<TimeSlotDto>();

            for (var i = 0; i < slotCount; i++)
            {
                slots.Add(new TimeSlotDto(
                    new TimeSpan(9 + i, 0, 0),
                    new TimeSpan(10 + i, 0, 0)
                ));
            }

            availabilities.Add(new AvailabilityDto(dayOfWeek, slots));
            return availabilities;
        }
    }
}