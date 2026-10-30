namespace TodoApp.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
