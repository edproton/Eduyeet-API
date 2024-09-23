using API.Models;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ActionResultExtensions
{
    public static ActionResult ToHttpActionResult<T>(this ErrorOr<T> result)
    {
        if (result.IsError)
        {
            var statusCode = GetStatusCode(result.Errors[0].Type);
            var errorResponse = new ErrorResponse
            {
                Errors = result.Errors
                    .Select(e => new ErrorDetail { Code = e.Code, Description = e.Description })
                    .ToList()
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = statusCode
            };
        }

        return result.Value switch
        {
            null => new NoContentResult(),
            _ => new OkObjectResult(result.Value)
        };
    }
    
    

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}