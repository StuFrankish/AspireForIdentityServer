namespace IdentityServer.Extensions.Options;

public class ConnectionStrings : ICustomOptions
{
    public string SqlServer { get; set; } = String.Empty;
    public string Redis { get; set; } = String.Empty;
}
