using Application.Features.CreatePerson;
using Domain.Enums;

namespace Application.UnitTests.Features.Commands.CreatePerson.Validators;

[TestClass]
public class CreatePersonCommandValidatorTests
{
    private readonly CreatePersonCommandValidator _validator = new();

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new CreatePersonCommand("",
            "test@example.com",
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Name cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsTooShort()
    {
        var command = new CreatePersonCommand("A",
            "test@example.com",
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("'Name' must be between 2 and 50 characters. You entered 1 characters.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenNameIsTooLong()
    {
        var command = new CreatePersonCommand(new string('A', 51),
            "test@example.com",
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("'Name' must be between 2 and 50 characters. You entered 51 characters.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new CreatePersonCommand("John Doe",
            "",
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenEmailIsInvalid()
    {
        var command = new CreatePersonCommand("John Doe",
            "invalid-email",
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenEmailIsTooLong()
    {
        var longEmail = new string('a', 90) + "@example.com"; // 101 characters
        var command = new CreatePersonCommand("John Doe",
            longEmail,
            "password",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email cannot exceed 100 characters.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenPasswordIsEmpty()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Password)
            .WithErrorMessage("Password cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenPasswordIsTooShort()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "short",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Password)
            .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenTypeIsInvalid()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "password123",
            "826",
            (PersonTypeEnum)99);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Type)
            .WithErrorMessage("Invalid person type.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenCountryCodeIsEmpty()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "password123",
            "",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CountryCode)
            .WithErrorMessage("Country code cannot be empty.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenCountryCodeIsNotThreeCharacters()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "password123",
            "12",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CountryCode)
            .WithErrorMessage("Country code must be 3 characters long.");
    }

    [TestMethod]
    public void ShouldHaveErrorWhenCountryCodeIsInvalid()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "password123",
            "999",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CountryCode)
            .WithErrorMessage("Invalid country code. The country code must correspond to a valid time zone.");
    }

    [TestMethod]
    public void ShouldNotHaveErrorWhenCommandIsValid()
    {
        var command = new CreatePersonCommand("John Doe",
            "test@example.com",
            "password123",
            "826",
            PersonTypeEnum.Student);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}