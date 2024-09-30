using Application.Features.SetTutorQualifications;
using Application.IntegrationTests.Shared;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

namespace Application.IntegrationTests.Commands;

[TestClass]
public class SetTutorQualificationsTests : IntegrationTestBase
{
    private Guid _tutorId;
    private List<Qualification> _qualifications = default!;

    protected override async Task SeedTestData()
    {
        var tutor = new Tutor { Name = "Dr. Jane Smith", TimeZoneId = "America/New_York" };
        await TutorRepository.AddAsync(tutor, default);

        var ibLearningSystem = new LearningSystem { Name = "International Baccalaureate (IB)" };
        await LearningSystemRepository.AddAsync(ibLearningSystem, default);

        var apLearningSystem = new LearningSystem { Name = "Advanced Placement (AP)" };
        await LearningSystemRepository.AddAsync(apLearningSystem, default);

        var igcseLearningSystem = new LearningSystem
            { Name = "International General Certificate of Secondary Education (IGCSE)" };
        await LearningSystemRepository.AddAsync(igcseLearningSystem, default);

        var ibMathSubject = new Subject { Name = "IB Mathematics", LearningSystem = ibLearningSystem };
        await SubjectRepository.AddAsync(ibMathSubject, default);

        var ibPhysicsSubject = new Subject { Name = "IB Physics", LearningSystem = ibLearningSystem };
        await SubjectRepository.AddAsync(ibPhysicsSubject, default);

        var apCalculusSubject = new Subject { Name = "AP Calculus", LearningSystem = apLearningSystem };
        await SubjectRepository.AddAsync(apCalculusSubject, default);

        var igcseMathSubject = new Subject { Name = "IGCSE Mathematics", LearningSystem = igcseLearningSystem };
        await SubjectRepository.AddAsync(igcseMathSubject, default);

        _qualifications = new List<Qualification>
        {
            new() { Name = "IB Mathematics HL", Subject = ibMathSubject },
            new() { Name = "IB Mathematics SL", Subject = ibMathSubject },
            new() { Name = "IB Physics HL", Subject = ibPhysicsSubject },
            new() { Name = "AP Calculus AB", Subject = apCalculusSubject },
            new() { Name = "AP Calculus BC", Subject = apCalculusSubject },
            new() { Name = "IGCSE Mathematics (Extended)", Subject = igcseMathSubject }
        };

        foreach (var qualification in _qualifications)
        {
            await QualificationRepository.AddAsync(qualification, default);
        }

        await UnitOfWork.SaveChangesAsync();
        _tutorId = tutor.Id;
    }

    [TestMethod]
    public async Task SetTutorQualifications_ShouldSucceed_WhenValidCommandIsProvided()
    {
        // Arrange
        var command = new SetTutorQualificationsCommand(
            _tutorId,
            new List<Guid> { _qualifications[0].Id, _qualifications[1].Id });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.TutorId.Should().Be(_tutorId);
        result.Value.QualificationIds.Should()
            .BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[1].Id });

        var updatedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, default);
        updatedTutor.Should().NotBeNull();
        updatedTutor!.AvailableQualifications.Should().HaveCount(2);
        updatedTutor.AvailableQualifications.Select(q => q.Id).Should()
            .BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[1].Id });
    }

    [TestMethod]
    public async Task SetTutorQualifications_ShouldFail_WhenTutorDoesNotExist()
    {
        // Arrange
        var command = new SetTutorQualificationsCommand(
            Guid.NewGuid(),
            new List<Guid> { _qualifications[0].Id });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Description.Should().Contain("was not found");
    }

    [TestMethod]
    public async Task SetTutorQualifications_ShouldFail_WhenQualificationDoesNotExist()
    {
        // Arrange
        var command = new SetTutorQualificationsCommand(
            _tutorId,
            new List<Guid> { _qualifications[0].Id, Guid.NewGuid() });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("One or more qualification IDs are invalid.");
    }

    [TestMethod]
    public async Task SetTutorQualifications_ShouldUpdateExistingQualifications()
    {
        // Arrange
        var initialCommand = new SetTutorQualificationsCommand(
            _tutorId,
            new List<Guid> { _qualifications[0].Id, _qualifications[1].Id });

        await Mediator.Send(initialCommand);

        var updateCommand = new SetTutorQualificationsCommand(
            _tutorId,
            new List<Guid> { _qualifications[1].Id, _qualifications[2].Id });

        // Act
        var result = await Mediator.Send(updateCommand);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.TutorId.Should().Be(_tutorId);
        result.Value.QualificationIds.Should()
            .BeEquivalentTo(new List<Guid> { _qualifications[1].Id, _qualifications[2].Id });

        var updatedTutor = await TutorRepository.GetByIdWithQualificationsAndAvailabilitiesAsync(_tutorId, default);
        updatedTutor.Should().NotBeNull();
        updatedTutor!.AvailableQualifications.Should().HaveCount(2);
        updatedTutor.AvailableQualifications.Select(q => q.Id).Should()
            .BeEquivalentTo(new List<Guid> { _qualifications[1].Id, _qualifications[2].Id });
    }
}