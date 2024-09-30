using Application.Features.CreateLearningSystem;

namespace Application.UnitTests.Features.Commands.CreateNewSystem.Validators;

[TestClass]
public class CreateLearningSystemValidatorTests
{
    private readonly CreateLearningSystemCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new CreateLearningSystemCommand("");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("System name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new CreateLearningSystemCommand(new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("System name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new CreateLearningSystemCommand("ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}