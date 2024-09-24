using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infra.ValueObjects;

public class ApplicationUser : IdentityUser
{
    public Guid PersonId { get; set; }

    public Person Person { get; set; } = null!;
}