using Domain.Entities.Shared;

namespace Domain.Entities;

public class LearningSystem : BaseEntity
{
    public string Name { get; set; } = default!;

    // Navigation properties
    public List<Subject> Subjects { get; set; } = [];
}

public class Subject : BaseEntity
{
    public string Name { get; set; } = default!;

    // Navigation properties
    public Guid LearningSystemId { get; set; }
    public LearningSystem LearningSystem { get; set; } = default!;
    public List<Qualification> Qualifications { get; set; } = [];
}

public class Qualification : BaseEntity
{
    public string Name { get; set; } = default!;
    
    // Navigation properties
    public Subject Subject { get; set; } = default!;
    public Guid SubjectId { get; set; }
    public Guid QualificationId { get; set; }
}