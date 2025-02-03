#nullable enable

using Duende.IdentityServer.Models;

namespace IdentityServer.Pages.Login;

public class ViewModel
{
    public bool AllowRememberLogin { get; set; } = true;
    public bool EnableLocalLogin { get; set; } = true;
    public bool EnablePasskeyLogin { get; set; } = true;

    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = [];
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

    public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
    public string? ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

    public Client? ClientContext { get; set; } = default;

    public class ExternalProvider(string authenticationScheme, string? displayName = null)
    {
        public string? DisplayName { get; set; } = displayName;
        public string AuthenticationScheme { get; set; } = authenticationScheme;
    }
}