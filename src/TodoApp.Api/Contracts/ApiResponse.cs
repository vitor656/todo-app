namespace TodoApp.Api.Contracts;

public sealed record ApiResponse<T>(bool Success, string Message, T? Data, IReadOnlyList<ApiError> Errors, string TraceId);
