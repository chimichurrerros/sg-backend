using BackEnd.Constants.Errors;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Extensions;

public static class ControllerExtensions
{
    public static ActionResult HandleValidationProblem(this ControllerBase controller, Result result)
    {
        return controller.BadRequest(new ValidationProblemDetails(result.Errors ?? new Dictionary<string, string[]>())
        {
            Title = string.IsNullOrWhiteSpace(result.ErrorMessage) ? ApplicationError.ValidationFailed : result.ErrorMessage
        });
    }

    public static ActionResult HandleNotFoundProblem(this ControllerBase controller, Result result)
    {
        return controller.NotFound(new ValidationProblemDetails(result.Errors ?? new Dictionary<string, string[]>())
        {
            Title = string.IsNullOrWhiteSpace(result.ErrorMessage) ? ApplicationError.NotFound : result.ErrorMessage
        });
    }
} 
