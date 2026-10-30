namespace TodoApp.Api.Contracts;

public sealed record ApiError(string Field, string Message);
