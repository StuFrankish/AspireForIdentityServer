namespace Client.Options;

public class PollyResilienceOptions : ICustomOptions
{
    public int AllowedEventsBeforeCircuitBreaker { get; set; }
    public int DurationOfBreakSeconds { get; set; }
    public int AllowedRetryCountBeforeFailure { get; set; }
}
