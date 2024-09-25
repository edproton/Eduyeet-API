using Domain.Entities.Shared;

namespace Domain.Entities;

public class LearningSystem : BaseEntity
{
    public required string Name { get; set; }

    public List<Subject> Subjects { get; set; } = [];
}

public class Subject : BaseEntity
{
    public required string Name { get; set; }
    
    public Guid LearningSystemId { get; set; }
    public required LearningSystem LearningSystem { get; set; }

    public List<Qualification> Qualifications { get; set; } = [];
}

public class Qualification : BaseEntity
{
    public required string Name { get; set; }
    public Guid QualificationId { get; set; }
    public required Subject Subject { get; set; }
}