using Application.Features.CreateLearningSystem;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Features.CreateNewSystem.Validators;

[TestClass]
public class CreateQualificationCommandValidatorTests
{
    private readonly CreateQualificationCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new CreateQualificationCommand("");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new CreateQualificationCommand(new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new CreateQualificationCommand("ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}