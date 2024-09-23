namespace Application.Features.RemoveSubjectFromSystem;

public class Handler(
    IUnitOfWork unitOfWork,
    ISubjectRepository subjectRepository)
    : IRequestHandler<RemoveSubjectFromSystemCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveSubjectFromSystemCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken);
        if (subject == null)
        {
            return Error.NotFound("SubjectNotFound", $"A subject with the ID '{request.SubjectId}' was not found.");
        }

        await subjectRepository.RemoveAsync(subject, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}