using TodoApp.Application.Abstractions;

namespace TodoApp.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
