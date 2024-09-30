using Application.Features.SetStudentQualifications;
using Application.IntegrationTests.Shared;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

namespace Application.IntegrationTests.Commands;

[TestClass]
public class SetStudentQualificationsTests : IntegrationTestBase
{
    private Guid _studentId;
    private List<Qualification> _qualifications = default!;

    protected override async Task SeedTestData()
    {
        var student = new Student { Name = "Alice Johnson", TimeZoneId = "America/New_York" };
        await StudentRepository.AddAsync(student, default);

        var ibLearningSystem = new LearningSystem { Name = "International Baccalaureate (IB)" };
        await LearningSystemRepository.AddAsync(ibLearningSystem, default);

        var aLevelLearningSystem = new LearningSystem { Name = "A-Levels" };
        await LearningSystemRepository.AddAsync(aLevelLearningSystem, default);

        var ibMathSubject = new Subject { Name = "IB Mathematics", LearningSystem = ibLearningSystem };
        await SubjectRepository.AddAsync(ibMathSubject, default);

        var ibPhysicsSubject = new Subject { Name = "IB Physics", LearningSystem = ibLearningSystem };
        await SubjectRepository.AddAsync(ibPhysicsSubject, default);

        var aLevelMathSubject = new Subject { Name = "A-Level Mathematics", LearningSystem = aLevelLearningSystem };
        await SubjectRepository.AddAsync(aLevelMathSubject, default);

        _qualifications = new List<Qualification>
        {
            new() { Name = "IB Mathematics HL", Subject = ibMathSubject },
            new() { Name = "IB Mathematics SL", Subject = ibMathSubject },
            new() { Name = "IB Physics HL", Subject = ibPhysicsSubject },
            new() { Name = "A-Level Further Mathematics", Subject = aLevelMathSubject }
        };

        foreach (var qualification in _qualifications)
        {
            await QualificationRepository.AddAsync(qualification, default);
        }

        await UnitOfWork.SaveChangesAsync();
        _studentId = student.Id;
    }

    [TestMethod]
    public async Task SetStudentQualifications_ShouldSucceed_WhenValidCommandIsProvided()
    {
        // Arrange
        var command = new SetStudentQualificationsCommand(
            _studentId,
            new List<Guid> { _qualifications[0].Id, _qualifications[2].Id }); // IB Mathematics HL and IB Physics HL

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StudentId.Should().Be(_studentId);
        result.Value.QualificationIds.Should().BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[2].Id });

        var updatedStudent = await StudentRepository.GetByIdWithQualificationsAsync(_studentId, default);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.InterestedQualifications.Should().HaveCount(2);
        updatedStudent.InterestedQualifications.Select(q => q.Id).Should().BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[2].Id });
    }

    [TestMethod]
    public async Task SetStudentQualifications_ShouldFail_WhenStudentDoesNotExist()
    {
        // Arrange
        var command = new SetStudentQualificationsCommand(
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
    public async Task SetStudentQualifications_ShouldFail_WhenQualificationDoesNotExist()
    {
        // Arrange
        var command = new SetStudentQualificationsCommand(
            _studentId,
            new List<Guid> { _qualifications[0].Id, Guid.NewGuid() });

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("One or more qualification IDs are invalid.");
    }

    [TestMethod]
    public async Task SetStudentQualifications_ShouldUpdateExistingQualifications()
    {
        // Arrange
        var initialCommand = new SetStudentQualificationsCommand(
            _studentId,
            new List<Guid> { _qualifications[0].Id, _qualifications[1].Id }); // IB Mathematics HL and SL

        await Mediator.Send(initialCommand);

        var updateCommand = new SetStudentQualificationsCommand(
            _studentId,
            new List<Guid> { _qualifications[1].Id, _qualifications[3].Id }); // IB Mathematics SL and A-Level Further Mathematics

        // Act
        var result = await Mediator.Send(updateCommand);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StudentId.Should().Be(_studentId);
        result.Value.QualificationIds.Should().BeEquivalentTo(new List<Guid> { _qualifications[1].Id, _qualifications[3].Id });

        var updatedStudent = await StudentRepository.GetByIdWithQualificationsAsync(_studentId, default);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.InterestedQualifications.Should().HaveCount(2);
        updatedStudent.InterestedQualifications.Select(q => q.Id).Should().BeEquivalentTo(new List<Guid> { _qualifications[1].Id, _qualifications[3].Id });
    }

    [TestMethod]
    public async Task SetStudentQualifications_ShouldAllowMixedQualifications()
    {
        // Arrange
        var command = new SetStudentQualificationsCommand(
            _studentId,
            new List<Guid> { _qualifications[0].Id, _qualifications[3].Id }); // IB Mathematics HL and A-Level Further Mathematics

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StudentId.Should().Be(_studentId);
        result.Value.QualificationIds.Should().BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[3].Id });

        var updatedStudent = await StudentRepository.GetByIdWithQualificationsAsync(_studentId, default);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.InterestedQualifications.Should().HaveCount(2);
        updatedStudent.InterestedQualifications.Select(q => q.Id).Should().BeEquivalentTo(new List<Guid> { _qualifications[0].Id, _qualifications[3].Id });
    }
}