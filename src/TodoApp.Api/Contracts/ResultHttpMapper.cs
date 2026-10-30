using TodoApp.Domain.Common;

namespace TodoApp.Api.Contracts;

public static class ResultHttpMapper
{
    public static IResult ToHttpResult<T>(Result<T> result, HttpContext httpContext, string successMessage)
    {
        if (result.IsFailure)
        {
            return ToFailureResult(result.Error!, httpContext);
        }

        var traceId = ApiResponseFactory.GetTraceId(httpContext);
        var response = ApiResponseFactory.Success(result.Value, successMessage, traceId);
        return Results.Ok(response);
    }

    public static IResult ToCreatedResult<T>(
        Result<T> result,
        HttpContext httpContext,
        string successMessage,
        Func<T, string> locationFactory)
    {
        if (result.IsFailure)
        {
            return ToFailureResult(result.Error!, httpContext);
        }

        var traceId = ApiResponseFactory.GetTraceId(httpContext);
        var response = ApiResponseFactory.Success(result.Value, successMessage, traceId);
        return Results.Created(locationFactory(result.Value), response);
    }

    public static IResult ToHttpResult(Result result, HttpContext httpContext, string successMessage)
    {
        if (result.IsFailure)
        {
            return ToFailureResult(result.Error!, httpContext);
        }

        var traceId = ApiResponseFactory.GetTraceId(httpContext);
        var response = ApiResponseFactory.Success<object?>(null, successMessage, traceId);
        return Results.Ok(response);
    }

    private static IResult ToFailureResult(Error error, HttpContext httpContext)
    {
        var traceId = ApiResponseFactory.GetTraceId(httpContext);
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var errors = error.FieldErrors.Select(f => new ApiError(f.Field, f.Message)).ToArray();
        var response = ApiResponseFactory.Failure<object?>(error.Message, errors, traceId);

        return Results.Json(response, statusCode: statusCode);
    }
}
