using HRMS.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            ArgumentException argumentException
                => Problem(detail: argumentException.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request"),
            InvalidOperationException invalidOperation
                => Problem(detail: invalidOperation.Message, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request"),
            _
                => Problem(detail: "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error")
        };
    }
}
