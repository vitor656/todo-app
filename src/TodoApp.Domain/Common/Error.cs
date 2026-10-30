namespace TodoApp.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
}

public sealed record FieldError(string Field, string Message);

public sealed class Error
{
    private Error(ErrorType type, string code, string message, IReadOnlyList<FieldError> fieldErrors)
    {
        Type = type;
        Code = code;
        Message = message;
        FieldErrors = fieldErrors;
    }

    public ErrorType Type { get; }

    public string Code { get; }

    public string Message { get; }

    public IReadOnlyList<FieldError> FieldErrors { get; }

    public static Error Validation(string code, string message, IReadOnlyList<FieldError> fieldErrors) =>
        new(ErrorType.Validation, code, message, fieldErrors);

    public static Error NotFound(string code, string message) =>
        new(ErrorType.NotFound, code, message, []);

    public static Error Conflict(string code, string message) =>
        new(ErrorType.Conflict, code, message, []);
}
