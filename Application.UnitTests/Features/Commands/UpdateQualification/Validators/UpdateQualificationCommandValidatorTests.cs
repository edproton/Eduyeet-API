using Application.Features.UpdateQualification;

namespace Application.UnitTests.Features.Commands.UpdateQualification.Validators;

[TestClass]
public class UpdateQualificationCommandValidatorTests
{
    private readonly UpdateQualificationCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateQualificationCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new UpdateQualificationCommand(Guid.NewGuid(), new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new UpdateQualificationCommand(Guid.NewGuid(), "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
