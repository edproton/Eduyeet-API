namespace Application.Features.DeleteLearningSystem;


public record DeleteLearningSystemCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

public class DeleteLearningSystemCommandValidator : AbstractValidator<DeleteLearningSystemCommand>
{
    public DeleteLearningSystemCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("System ID cannot be empty.");
    }
}

public class Handler(
    IUnitOfWork unitOfWork,
    ILearningSystemRepository systemRepository)
    : IRequestHandler<DeleteLearningSystemCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteLearningSystemCommand request,
        CancellationToken cancellationToken)
    {
        var system = await systemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (system == null)
        {
            return Error.NotFound("LearningSystemNotFound", $"A learning system with the ID '{request.Id}' was not found.");
        }

        await systemRepository.RemoveAsync(system, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new Deleted();
    }
}