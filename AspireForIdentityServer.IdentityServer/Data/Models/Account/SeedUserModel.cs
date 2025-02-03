namespace IdentityServer.Data.Models.Account;

public class SeedUserModel
{
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}
