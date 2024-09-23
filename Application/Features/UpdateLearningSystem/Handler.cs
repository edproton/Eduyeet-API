namespace Application.Features.UpdateLearningSystem;

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository,
    ISubjectRepository subjectRepository,
    IQualificationRepository qualificationRepository)
    : IRequestHandler<UpdateLearningSystemCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateLearningSystemCommand request,
        CancellationToken cancellationToken)
    {
        var existingSystem = await systemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingSystem == null)
        {
            return Error.NotFound("LearningSystemNotFound",
                $"A learning system with the ID '{request.Id}' was not found.");
        }

        // Update the name if it has changed
        if (existingSystem.Name != request.Name)
        {
            var systemWithSameName = await systemRepository.GetByNameAsync(request.Name, cancellationToken);
            if (systemWithSameName != null && systemWithSameName.Id != request.Id)
            {
                return Error.Conflict("LearningSystemNameAlreadyExists",
                    $"A learning system with the name '{request.Name}' already exists.");
            }

            existingSystem.Name = request.Name;
        }

        // Update subjects
        await UpdateSubjectsAsync(existingSystem, request.Subjects, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }

    private async Task UpdateSubjectsAsync(
        LearningSystem system,
        List<UpdateSubjectCommand> subjectCommands,
        CancellationToken cancellationToken)
    {
        var existingSubjects
            = (await subjectRepository.GetByLearningSystemIdAsync(system.Id, cancellationToken)).ToList();

        foreach (var subjectCommand in subjectCommands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingSubject = existingSubjects.FirstOrDefault(s => s.Id == subjectCommand.Id);
            if (existingSubject == null)
            {
                // Add new subject
                var newSubject = new Subject { Name = subjectCommand.Name, LearningSystem = system };
                await subjectRepository.AddAsync(newSubject, cancellationToken);
                await UpdateQualificationsAsync(newSubject, subjectCommand.Qualifications, cancellationToken);
            }
            else
            {
                // Update existing subject
                existingSubject.Name = subjectCommand.Name;
                await subjectRepository.UpdateAsync(existingSubject, cancellationToken);
                await UpdateQualificationsAsync(existingSubject, subjectCommand.Qualifications, cancellationToken);
            }
        }

        // Remove subjects that are not in the update command
        var subjectsToRemove = existingSubjects.Where(s => !subjectCommands.Any(sc => sc.Id == s.Id));
        foreach (var subject in subjectsToRemove)
        {
            await subjectRepository.RemoveAsync(subject, cancellationToken);
        }
    }

    private async Task UpdateQualificationsAsync(
        Subject subject,
        List<UpdateQualificationCommand> qualificationCommands,
        CancellationToken cancellationToken)
    {
        var existingQualifications
            = (await qualificationRepository.GetBySubjectIdAsync(subject.Id, cancellationToken)).ToList();

        foreach (var qualificationCommand in qualificationCommands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingQualification = existingQualifications.FirstOrDefault(q => q.Id == qualificationCommand.Id);
            if (existingQualification == null)
            {
                // Add new qualification
                var newQualification = new Qualification { Name = qualificationCommand.Name, Subject = subject };
                await qualificationRepository.AddAsync(newQualification, cancellationToken);
            }
            else
            {
                // Update existing qualification
                existingQualification.Name = qualificationCommand.Name;
                await qualificationRepository.UpdateAsync(existingQualification, cancellationToken);
            }
        }

        // Remove qualifications that are not in the update command
        var qualificationsToRemove = existingQualifications.Where(q => !qualificationCommands.Any(qc => qc.Id == q.Id));
        foreach (var qualification in qualificationsToRemove)
        {
            await qualificationRepository.RemoveAsync(qualification, cancellationToken);
        }
    }
}