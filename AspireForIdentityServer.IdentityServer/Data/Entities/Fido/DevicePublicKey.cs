﻿namespace IdentityServer.Data.Entities.Fido;

public sealed class DevicePublicKey
{
    public required byte[] PublicKeyCredentialId { get; set; }
    public required byte[] Value { get; set; }
}
