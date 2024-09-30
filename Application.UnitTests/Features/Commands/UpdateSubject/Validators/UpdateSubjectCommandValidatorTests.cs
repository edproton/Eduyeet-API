using Application.Features.UpdateSubject;

namespace Application.UnitTests.Features.Commands.UpdateSubject.Validators;

[TestClass]
public class UpdateSubjectCommandValidatorTests
{
    private readonly UpdateSubjectCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateSubjectCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new UpdateSubjectCommand(Guid.NewGuid(), new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Subject name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new UpdateSubjectCommand(Guid.NewGuid(), "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}