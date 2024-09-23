namespace API.Models;

public class ErrorResponse
{
    public List<ErrorDetail> Errors { get; set; } = new();
}

public class ErrorDetail
{
    public required string Code { get; set; }

    public required string Description { get; set; }
}