namespace AppAutomation.Abstractions;

public readonly record struct UiWaitResult<T>(bool Success, T Value, TimeSpan Elapsed);
