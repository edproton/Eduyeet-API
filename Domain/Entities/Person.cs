using Domain.Entities.Shared;
using Domain.Enums;

namespace Domain.Entities;

public interface IHasQualifications
{
    IEnumerable<Guid> GetQualificationIds();
}

public class Person : BaseEntity
{
    public PersonTypeEnum Type { get; set; }

    public required string Name { get; set; }

    public Person(PersonTypeEnum type)
    {
        Type = type;
    }

    public Person()
    {
        
    }
    
    public bool IsTutor()
    {
        return Type == PersonTypeEnum.Tutor;
    }
    
    public bool IsStudent()
    {
        return Type == PersonTypeEnum.Student;
    }
}