using Application.Features.SetTutorAvailability;

namespace Application.UnitTests.Features.Commands.SetTutorAvailability.Validators;

[TestClass]
public class TimeSlotDtoValidatorTests
{
    private readonly TimeSlotDtoValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenStartTimeIsAfterEndTime()
    {
        var timeSlot = new TimeSlotDto(new TimeSpan(17, 0, 0), new TimeSpan(9, 0, 0));
        var result = _validator.TestValidate(timeSlot);
        result.ShouldHaveValidationErrorFor(t => t.StartTime)
            .WithErrorMessage("Start time must be before end time.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenTimeSlotIsValid()
    {
        var timeSlot = new TimeSlotDto(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
        var result = _validator.TestValidate(timeSlot);
        result.ShouldNotHaveAnyValidationErrors();
    }
}