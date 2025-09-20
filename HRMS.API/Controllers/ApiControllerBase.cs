using HRMS.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected static PagedRequest? BuildPagedRequest(int? page, int? size, string? search)
    {
        var hasPagingParameters = page.HasValue || size.HasValue || !string.IsNullOrWhiteSpace(search);
        if (!hasPagingParameters)
        {
            return null;
        }

        var request = new PagedRequest();

        if (page.HasValue)
        {
            request.Page = page.Value;
        }

        if (size.HasValue)
        {
            request.PageSize = size.Value;
        }

        request.SearchTerm = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        return request;
    }

    protected IActionResult HandleException(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException invalidOperation when invalidOperation.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => Problem(detail: invalidOperation.Message, statusCode: StatusCodes.Status404NotFound, title: "Not Found"),
            InvalidOperationException invalidOperation when invalidOperation.Message.Contains("exist", StringComparison.OrdinalIgnoreCase)
                or invalidOperation.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                => Problem(detail: invalidOperation.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflict"),
            ArgumentException argumentException
                => Problem(detail: argumentException.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request"),
            InvalidOperationException invalidOperation
                => Problem(detail: invalidOperation.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request"),
            _
                => Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error")
        };
    }

    protected IActionResult HandleAndLogException(Exception exception, ILogger logger, string operation, string entityName)
    {
        switch (exception)
        {
            case ArgumentException argumentException:
                logger.LogWarning(argumentException, "Validation error while {Operation} {Entity}", operation, entityName);
                return Problem(detail: argumentException.Message, statusCode: StatusCodes.Status400BadRequest, title: "Validation Error");
            case InvalidOperationException invalidOperation:
                var (statusCode, title) = ResolveStatusFromMessage(invalidOperation.Message);
                logger.LogWarning(invalidOperation, "Business rule violation while {Operation} {Entity}", operation, entityName);
                return Problem(detail: invalidOperation.Message, statusCode: statusCode, title: title);
            default:
                logger.LogError(exception, "Unexpected error while {Operation} {Entity}", operation, entityName);
                var safeMessage = $"An unexpected error occurred while {operation.ToLowerInvariant()} the {entityName.ToLowerInvariant()}.";
                return Problem(detail: safeMessage, statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }
    }

    private static (int StatusCode, string Title) ResolveStatusFromMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return (StatusCodes.Status400BadRequest, "Bad Request");
        }

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status404NotFound, "Not Found");
        }

        if (message.Contains("exist", StringComparison.OrdinalIgnoreCase) || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            return (StatusCodes.Status409Conflict, "Conflict");
        }

        return (StatusCodes.Status400BadRequest, "Bad Request");
    }
}
