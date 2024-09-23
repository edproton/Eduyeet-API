namespace Application.Features.AddSubjectToLearningSystem;

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository,
    ISubjectRepository subjectRepository)
    : IRequestHandler<AddSubjectToSystemCommand, ErrorOr<AddSubjectToSystemCommandResponse>>
{
    public async Task<ErrorOr<AddSubjectToSystemCommandResponse>> Handle(
        AddSubjectToSystemCommand request,
        CancellationToken cancellationToken)
    {
        var system = await systemRepository.GetByIdAsync(request.LearningSystemId, cancellationToken);
        if (system == null)
        {
            return Error.NotFound("LearningSystemNotFound", $"A learning system with the ID '{request.LearningSystemId}' was not found.");
        }

        var existingSubject = await subjectRepository.GetByNameAndSystemIdAsync(
            request.Name,
            request.LearningSystemId,
            cancellationToken);

        if (existingSubject != null)
        {
            return Error.Conflict("SubjectAlreadyExists", 
                $"A subject with the name '{request.Name}' already exists in the learning system with ID '{request.LearningSystemId}'.");
        }

        var subject = new Subject { Name = request.Name, LearningSystem = system };
        await subjectRepository.AddAsync(subject, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddSubjectToSystemCommandResponse(subject.Id);
    }
}


