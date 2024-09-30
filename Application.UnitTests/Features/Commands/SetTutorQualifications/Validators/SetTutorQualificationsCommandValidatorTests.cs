using Application.Features.SetTutorQualifications;

namespace Application.UnitTests.Features.Commands.SetTutorQualifications.Validators;

[TestClass]
public class SetTutorQualificationsCommandValidatorTests
{
    private readonly SetTutorQualificationsCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenPersonIdIsEmpty()
    {
        var command = new SetTutorQualificationsCommand(Guid.Empty, new List<Guid> { Guid.NewGuid() });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PersonId)
            .WithErrorMessage("Tutor ID is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenQualificationIdsIsEmpty()
    {
        var command = new SetTutorQualificationsCommand(Guid.NewGuid(), new List<Guid>());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.QualificationIds)
            .WithErrorMessage("At least one qualification is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new SetTutorQualificationsCommand(
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}