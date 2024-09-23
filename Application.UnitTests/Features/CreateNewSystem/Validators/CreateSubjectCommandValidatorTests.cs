using Application.Features.CreateLearningSystem;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Features.CreateNewSystem.Validators;

[TestClass]
public class CreateSubjectCommandValidatorTests
{
    private readonly CreateSubjectCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new CreateSubjectCommand("", []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new CreateSubjectCommand(new string('a', 101), []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenQualificationsIsEmpty()
    {
        var command = new CreateSubjectCommand("ValidName", []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Qualifications)
            .WithErrorMessage("At least one qualification is required for each subject.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new CreateSubjectCommand("ValidName", [new("ValidQualification")]);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}