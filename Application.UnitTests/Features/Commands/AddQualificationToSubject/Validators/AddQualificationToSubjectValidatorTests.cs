using Application.Features.AddQualificationToSubject;

namespace Application.UnitTests.Features.Commands.AddQualificationToSubject.Validators;

[TestClass]
public class AddQualificationToSubjectValidatorTests
{
    private readonly AddQualificationToSubjectCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenSubjectIdIsEmpty()
    {
        var command = new AddQualificationToSubjectCommand(Guid.Empty, "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SubjectId)
            .WithErrorMessage("Subject ID is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new AddQualificationToSubjectCommand(Guid.NewGuid(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameExceeds100Characters()
    {
        var command = new AddQualificationToSubjectCommand(Guid.NewGuid(), new string('a', 101));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Qualification name cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new AddQualificationToSubjectCommand(Guid.NewGuid(), "ValidName");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}