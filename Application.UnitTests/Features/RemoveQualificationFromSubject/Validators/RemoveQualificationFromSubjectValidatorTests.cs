using Application.Features.RemoveQualificationFromSubject;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Features.RemoveQualificationFromSubject.Validators;

[TestClass]
public class RemoveQualificationFromSubjectValidatorTests
{
    private readonly RemoveQualificationFromSubjectCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenQualificationIdIsEmpty()
    {
        var command = new RemoveQualificationFromSubjectCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.QualificationId)
            .WithErrorMessage("Qualification ID is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new RemoveQualificationFromSubjectCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}