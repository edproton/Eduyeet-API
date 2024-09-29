namespace Application.Features.SetTutorQualifications;

public record SetTutorQualificationsCommand(
    Guid PersonId,
    List<Guid> QualificationIds) : IRequest<ErrorOr<SetTutorQualificationsResponse>>;

public class SetTutorQualificationsCommandValidator : AbstractValidator<SetTutorQualificationsCommand>
{
    public SetTutorQualificationsCommandValidator()
    {
        RuleFor(c => c.PersonId).NotEmpty().WithMessage("Tutor ID is required.");
        RuleFor(c => c.QualificationIds).NotEmpty().WithMessage("At least one qualification is required.");
    }
}

public class SetTutorQualificationsHandler(
    ITutorRepository tutorRepository,
    IQualificationRepository qualificationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetTutorQualificationsCommand, ErrorOr<SetTutorQualificationsResponse>>
{
    public async Task<ErrorOr<SetTutorQualificationsResponse>> Handle(
        SetTutorQualificationsCommand request,
        CancellationToken cancellationToken)
    {
        var tutor = await tutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(request.PersonId, cancellationToken);
        if (tutor == null)
        {
            return Error.NotFound("TutorNotFound", $"A tutor with the ID '{request.PersonId}' was not found.");
        }

        var qualifications = await qualificationRepository.GetQualificationsByIdsAsync(request.QualificationIds, cancellationToken);
        if (qualifications.Count != request.QualificationIds.Count)
        {
            return Error.Validation("InvalidQualifications", "One or more qualification IDs are invalid.");
        }

        tutor.AvailableQualifications = qualifications;

        await tutorRepository.UpdateAsync(tutor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetTutorQualificationsResponse(tutor.Id, qualifications.Select(q => q.Id).ToList());
    }
}

public record SetTutorQualificationsResponse(Guid TutorId, List<Guid> QualificationIds);