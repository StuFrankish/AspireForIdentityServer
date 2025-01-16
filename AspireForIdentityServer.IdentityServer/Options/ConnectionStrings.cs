namespace IdentityServer.Extensions.Options;

public class ConnectionStrings : ICustomOptions
{
    public string SqlServer { get; init; } = String.Empty;
    public string Redis { get; init; } = String.Empty;
}
