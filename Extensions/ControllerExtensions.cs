using BackEnd.Constants.Errors;
using BackEnd.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Extensions;

public static class ControllerExtensions
{
    public static ActionResult HandleBadRequestProblem(this ControllerBase controller, Result result)
    {
        return controller.BadRequest(new ProblemDetails
        {
            Title = ApplicationError.BadRequest,
            Status = StatusCodes.Status400BadRequest,
            Detail = result.ErrorMessage
        });
    }

    public static ActionResult HandleValidationProblem(this ControllerBase controller, Result result)
    {
        if (result.Errors != null)
            return controller.BadRequest(new ValidationProblemDetails(result.Errors));

        return controller.BadRequest(new ProblemDetails
        {
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = result.ErrorMessage
        });
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

    public static ActionResult HandleNotFoundProblem(this ControllerBase controller, Result result, int? id)
    {
        var detail = result.ErrorMessage;
        if (id.HasValue) detail = string.Format($"{detail}{SalesOrderError.IdSuffix}", id.Value);

        return controller.NotFound(new ProblemDetails
        {
            Title = ApplicationError.NotFound,
            Status = StatusCodes.Status404NotFound,
            Detail = detail
        });
    }

    public static ActionResult HandleConflictProblem(this ControllerBase controller, Result result)
    {
        return controller.Conflict(new ProblemDetails
        {
            Title = ApplicationError.Conflict,
            Status = StatusCodes.Status409Conflict,
            Detail = result.ErrorMessage
        });
    }

    public static ActionResult HandleServerError(this ControllerBase controller, string title, Result result, int? id = null)
    {
        var detail = result.ErrorMessage;
        if (id.HasValue) detail = string.Format($"{detail}{SalesOrderError.IdSuffix}", id.Value);

        return controller.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status500InternalServerError,
            Detail = detail
        });
    }
}
