// using Application.Features.CreateBooking;
// using Application.Features.SetTutorAvailability;
// using Application.IntegrationTests.Shared;
// using Application.IntegrationTests.Shared.Builders;
// using Domain.Entities;
// using FluentAssertions;
// using NodaTime;
//
// namespace Application.IntegrationTests.Commands.CreateBookingTests;
//
// [TestClass]
// public class CreateBookingTests : IntegrationTestBase
// {
//     [TestMethod]
//     public async Task CreateBooking_ShouldSucceed_WhenTutorIsAvailable()
//     {
//         // Arrange
//         var (student, tutor, qualification) = await CreateStudentTutorAndQualification("Europe/Lisbon", "America/New_York");
//
//         // Set tutor availability for today (Monday) from 2 PM to 4 PM New York time
//         var tutorAvailability = new List<AvailabilityDto>
//         {
//             new AvailabilityDto(DayOfWeek.Monday, new List<TimeSlotDto>
//             {
//                 new TimeSlotDto(new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)) 
//             })
//         };
//         await SetTutorAvailability(tutor.Id, tutorAvailability);
//
//         // Student in Lisbon wants to book at 8 PM Lisbon time (which is 3 PM New York time)
//         var bookingStartTime = DateTime.Now.Date.AddHours(20); // 8 PM Lisbon time today
//         var command = new CreateBookingCommand(student.Id, tutor.Id, qualification.Id, bookingStartTime);
//
//         // Act
//         var result = await Mediator.Send(command);
//
//         // Assert
//         result.IsError.Should().BeFalse();
//         var booking = result.Value;
//         booking.StudentId.Should().Be(student.Id);
//         booking.TutorId.Should().Be(tutor.Id);
//         booking.QualificationId.Should().Be(qualification.Id);
//
//         // Assert booking times in student's local time
//         var studentLocalTimezone = DateTimeZoneProviders.Tzdb["Europe/Lisbon"];
//         var expectedStudentStartTime = LocalDateTime.FromDateTime(bookingStartTime)
//             .InZoneLeniently(studentLocalTimezone)
//             .ToDateTimeUnspecified();
//         var expectedStudentEndTime = expectedStudentStartTime.AddHours(1);
//
//         booking.StartTime.Should().Be(expectedStudentStartTime);
//         booking.EndTime.Should().Be(expectedStudentEndTime);
//     }
//
//     [TestMethod]
//     public async Task CreateBooking_ShouldFail_WhenTutorIsUnavailable()
//     {
//         // Arrange
//         var (student, tutor, qualification) = await CreateStudentTutorAndQualification("Europe/Lisbon", "America/New_York");
//
//         // Set tutor availability for today (Monday) from 9 AM to 11 AM New York time
//         var tutorAvailability = new List<AvailabilityDto>
//         {
//             new AvailabilityDto(DayOfWeek.Monday, new List<TimeSlotDto>
//             {
//                 new TimeSlotDto(new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0))
//             })
//         };
//         await SetTutorAvailability(tutor.Id, tutorAvailability);
//
//         // Student in Lisbon wants to book at 8 PM Lisbon time (which is 3 PM New York time)
//         var bookingStartTime = DateTime.Now.Date.AddHours(20); 
//         var command = new CreateBookingCommand(student.Id, tutor.Id, qualification.Id, bookingStartTime);
//
//         // Act
//         var result = await Mediator.Send(command);
//
//         // Assert
//         result.IsError.Should().BeTrue();
//         result.FirstError.Should().Be(Errors.Booking.TutorNotAvailable); 
//     }
//
//     // ... other test cases covering various scenarios
//
//     private async Task<(Student, Tutor, Qualification)> CreateStudentTutorAndQualification(
//         string studentTimeZoneId, string tutorTimeZoneId)
//     {
//         var learningSystem = new LearningSystemBuilder().Build();
//         await LearningSystemRepository.AddAsync(learningSystem, default);
//
//         var subject = new SubjectBuilder()
//             .WithLearningSystem(learningSystem)
//             .Build();
//         await SubjectRepository.AddAsync(subject, default);
//
//         var qualification = new QualificationBuilder()
//             .WithSubject(subject)
//             .Build();
//         await QualificationRepository.AddAsync(qualification, default);
//
//         var student = new StudentBuilder()
//             .WithTimeZoneId(studentTimeZoneId)
//             .WithInterestedQualification(qualification)
//             .Build();
//         await StudentRepository.AddAsync(student, default);
//
//         var tutor = new TutorBuilder()
//             .WithTimeZoneId(tutorTimeZoneId)
//             .WithQualification(qualification)
//             .Build();
//         await TutorRepository.AddAsync(tutor, default);
//
//         await UnitOfWork.SaveChangesAsync();
//
//         return (student, tutor, qualification);
//     }
//
//     private async Task SetTutorAvailability(Guid tutorId, List<AvailabilityDto> availabilities)
//     {
//         var command = new SetTutorAvailabilityCommand(tutorId, availabilities);
//         var result = await Mediator.Send(command);
//         
//         result.IsError.Should().BeFalse();
//     }
// }