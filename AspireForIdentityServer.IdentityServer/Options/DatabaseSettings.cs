namespace IdentityServer.Extensions.Options;

public class DatabaseSettings : ICustomOptions
{
    public int MinPoolSize { get; init; } = 5;
    public int MaxPoolSize { get; init; } = 100;
}
