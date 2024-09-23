using Application.Features.RemoveSubjectFromSystem;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Features.RemoveSubjectFromSystem.Validators;

[TestClass]
public class RemoveSubjectFromSystemValidatorTests
{
    private readonly RemoveSubjectFromSystemCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenSubjectIdIsEmpty()
    {
        var command = new RemoveSubjectFromSystemCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SubjectId)
            .WithErrorMessage("Subject ID is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new RemoveSubjectFromSystemCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}