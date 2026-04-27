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

    public static ActionResult HandleServerError(this ControllerBase controller, string title, Result result, int? id = null)
    {
        var detail = result.ErrorMessage;
        if (id.HasValue) detail = string.Format($"{detail}{SalesOrderError.IdSuffix}", id.Value);

        return controller.Problem(
            title: title,
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
