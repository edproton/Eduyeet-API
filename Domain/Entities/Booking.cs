using Domain.Entities.Shared;

namespace Domain.Entities;

public class Booking : BaseEntity
{
    public DateTime Start { get; set; }
    
    public DateTime End { get; set; }
    
    public required string Location { get; set; }

    public required Qualification Qualification { get; set; }
    
    public required Tutor Tutor { get; set; }
    
    public required Student Student { get; set; }
}
