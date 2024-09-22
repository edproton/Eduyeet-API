using Domain.Entities.Shared;
using Domain.Enums;

namespace Domain.Entities;

public abstract class Person(PersonTypeEnum type) : BaseEntity
{
    public PersonTypeEnum Type { get; set; } = type;

    public required string Name { get; set; }
    
    public required string Email { get; set; }
}