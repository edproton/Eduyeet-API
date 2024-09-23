namespace Application.Features.RemoveQualificationFromSubject;

public class Handler(
    IUnitOfWork unitOfWork,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<RemoveQualificationFromSubjectCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveQualificationFromSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification == null)
        {
            return Error.NotFound("QualificationNotFound",
                $"A qualification with the ID '{request.QualificationId}' was not found.");
        }

        await qualificationRepository.RemoveAsync(qualification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}