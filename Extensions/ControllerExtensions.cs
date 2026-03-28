using BackEnd.Constants.Errors;
using BackEnd.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Extensions;

public static class ControllerExtensions
{
    public static ActionResult HandleValidationProblem(this ControllerBase controller, Result result)
    {
        return controller.BadRequest(new ValidationProblemDetails(result.Errors!));
    }

    public static ActionResult HandleNotFoundProblem(this ControllerBase controller, Result result)
    {
        return controller.NotFound(new ProblemDetails
        {
            Title = ApplicationError.NotFound,
            Status = StatusCodes.Status404NotFound,
            Detail = result.ErrorMessage
        });
    }
} 
