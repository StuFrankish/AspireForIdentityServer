namespace IdentityServer.Data.Entities.Fido;

public sealed class PublicKeyCredential
{
    public required byte[] Id { get; set; }
    public required byte[] PublicKey { get; set; }
    public required uint SignatureCounter { get; set; }
    public bool IsBackupEligible { get; set; }
    public bool IsBackedUp { get; set; }
    public required byte[] AttestationObject { get; set; }
    public required byte[] AttestationClientDataJson { get; set; }
    public required string AttestationFormat { get; set; }
    public required Guid Aaguid { get; set; }
    public required string UserId { get; set; }

    public ICollection<AuthenticatorTransport> AuthenticatorTransports { get; set; } = [];
    public ICollection<DevicePublicKey> DevicePublicKeys { get; set; } = [];
}
