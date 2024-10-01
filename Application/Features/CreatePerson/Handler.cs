using Application.Services;
using Domain.Enums;

namespace Application.Features.CreatePerson;

public record CreatePersonCommand(
    string Name,
    string Email,
    string Password,
    string CountryCode,
    PersonTypeEnum Type) : IRequest<ErrorOr<Created>>;

public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    public CreatePersonCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .Length(2, 50);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(c => c.Type)
            .IsInEnum().WithMessage("Invalid person type.");
        
        RuleFor(c => c.CountryCode)
            .NotEmpty().WithMessage("Country code cannot be empty.")
            .Length(3).WithMessage("Country code must be 3 characters long.")
            .Must(code => TimeZoneService.GetTimeZoneByCountryCode(code) != null)
            .WithMessage("Invalid country code. The country code must correspond to a valid time zone.");
    }
}

public class CreatePersonCommandHandler(
    IUnitOfWork unitOfWork,
    ITutorRepository tutorRepository,
    IStudentRepository studentRepository,
    IIdentityService identityService)
    : IRequestHandler<CreatePersonCommand, ErrorOr<Created>>
{
    public async Task<ErrorOr<Created>> Handle(
        CreatePersonCommand request,
        CancellationToken cancellationToken)
    {
        Person person = request.Type switch
        {
            PersonTypeEnum.Tutor => new Tutor { Name = request.Name, TimeZoneId = TimeZoneService.GetTimeZoneByCountryCode(request.CountryCode)! },
            PersonTypeEnum.Student => new Student { Name = request.Name, TimeZoneId = TimeZoneService.GetTimeZoneByCountryCode(request.CountryCode)! },
            _ => throw new ArgumentException("Invalid person type", nameof(request.Type))
        };
        
        switch (request.Type)
        {
            case PersonTypeEnum.Tutor:
                await tutorRepository.AddAsync((person as Tutor)!, cancellationToken);
                break;
            case PersonTypeEnum.Student:
                await studentRepository.AddAsync((person as Student)!, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Type), request.Type, "Invalid person type");
        }

        var registrationResult = await identityService.RegisterUserAsync(
            person.Id,
            request,
            cancellationToken);

        if (registrationResult.IsError)
        {
            return registrationResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}