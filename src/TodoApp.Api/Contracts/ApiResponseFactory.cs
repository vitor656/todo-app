using System.Diagnostics;

namespace TodoApp.Api.Contracts;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T? data, string message, string traceId) =>
        new(true, message, data, [], traceId);

    public static ApiResponse<T> Failure<T>(string message, IReadOnlyList<ApiError> errors, string traceId) =>
        new(false, message, default, errors, traceId);

    public static string GetTraceId(HttpContext httpContext) =>
        Activity.Current?.TraceId.ToHexString() ?? httpContext.TraceIdentifier;
}
