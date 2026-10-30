using System.Reflection;

namespace TodoApp.Application;

public sealed class ApplicationAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(ApplicationAssemblyMarker).Assembly;
}
