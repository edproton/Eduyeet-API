using Application.Features.SetTutorAvailability;

namespace Application.UnitTests.Features.Commands.SetTutorAvailability.Validators;

[TestClass]
public class SetTutorAvailabilityCommandValidatorTests
{
    private readonly SetTutorAvailabilityCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenPersonIdIsEmpty()
    {
        var command = new SetTutorAvailabilityCommand(Guid.Empty, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PersonId)
            .WithErrorMessage("Person ID is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenAvailabilitiesIsEmpty()
    {
        var command = new SetTutorAvailabilityCommand(Guid.NewGuid(), []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Availabilities)
            .WithErrorMessage("At least one availability is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new SetTutorAvailabilityCommand(
            Guid.NewGuid(),
            [
                new(DayOfWeek.Monday,
                    [new(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))])
            ]);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}