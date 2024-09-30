using Application.Features.SetTutorAvailability;

namespace Application.UnitTests.Features.Commands.SetTutorAvailability.Validators;

[TestClass]
public class AvailabilityDtoValidatorTests
{
    private readonly AvailabilityDtoValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenTimeSlotsIsEmpty()
    {
        var availability = new AvailabilityDto(DayOfWeek.Monday, []);
        var result = _validator.TestValidate(availability);
        result.ShouldHaveValidationErrorFor(a => a.TimeSlots)
            .WithErrorMessage("At least one time slot is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenAvailabilityIsValid()
    {
        var availability = new AvailabilityDto(
            DayOfWeek.Monday,
            [new(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))]);
        var result = _validator.TestValidate(availability);
        result.ShouldNotHaveAnyValidationErrors();
    }
}