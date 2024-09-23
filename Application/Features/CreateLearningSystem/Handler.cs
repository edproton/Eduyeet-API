namespace Application.Features.CreateLearningSystem;

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository,
    ISubjectRepository subjectRepository,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<CreateLearningSystemCommand, ErrorOr<CreateLearningSystemCommandResponse>>
{
    public async Task<ErrorOr<CreateLearningSystemCommandResponse>> Handle(
        CreateLearningSystemCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await systemRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingSystem != null)
        {
            return Error.Conflict("LearningSystemAlreadyExists",
                $"A learning system with the name '{request.Name}' already exists.");
        }

        var system = new LearningSystem { Name = request.Name };

        await systemRepository.AddAsync(system, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLearningSystemCommandResponse(system.Id);
    }
}