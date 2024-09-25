using Domain.Entities.Shared;
using Domain.Enums;

namespace Domain.Entities;

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
}