using Application.Features.DeleteLearningSystem;

namespace Application.UnitTests.Features.Commands.DeleteLearningSystem.Validators;

[TestClass]
public class DeleteLearningSystemCommandValidatorTests
{
    private readonly DeleteLearningSystemCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenIdIsEmpty()
    {
        var command = new DeleteLearningSystemCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id)
            .WithErrorMessage("System ID cannot be empty.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new DeleteLearningSystemCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}