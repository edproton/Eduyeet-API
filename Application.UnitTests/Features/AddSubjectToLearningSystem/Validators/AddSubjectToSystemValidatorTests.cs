using Application.Features.AddSubjectToLearningSystem;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Features.AddSubjectToLearningSystem.Validators;

[TestClass]
public class AddSubjectToSystemValidatorTests
{
    private readonly AddSubjectToSystemCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenLearningSystemIdIsEmpty()
    {
        var command = new AddSubjectToSystemCommand(Guid.Empty, "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.LearningSystemId)
            .WithErrorMessage("Learning system ID is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new AddSubjectToSystemCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new AddSubjectToSystemCommand(Guid.NewGuid(), new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new AddSubjectToSystemCommand(Guid.NewGuid(), "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}