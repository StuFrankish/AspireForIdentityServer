namespace IdentityServer.Models.Fido;

public sealed class AuthenticatorTransport
{
    public required byte[] PublicKeyCredentialId { get; set; }
    public required Fido2NetLib.Objects.AuthenticatorTransport Value { get; set; }
}
