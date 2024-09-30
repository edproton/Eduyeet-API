using Application.Features.SetStudentQualifications;

namespace Application.UnitTests.Features.Commands.SetStudentQualifications.Validators;

[TestClass]
public class SetStudentQualificationsCommandValidatorTests
{
    private readonly SetStudentQualificationsCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenPersonIdIsEmpty()
    {
        var command = new SetStudentQualificationsCommand(Guid.Empty, new List<Guid> { Guid.NewGuid() });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PersonId)
            .WithErrorMessage("Student ID is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenQualificationIdsIsEmpty()
    {
        var command = new SetStudentQualificationsCommand(Guid.NewGuid(), new List<Guid>());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.QualificationIds)
            .WithErrorMessage("At least one qualification is required.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new SetStudentQualificationsCommand(
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}