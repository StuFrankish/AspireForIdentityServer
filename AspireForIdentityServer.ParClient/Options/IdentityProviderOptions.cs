﻿using System;

namespace Client.Options;

public class IdentityProviderOptions : ICustomOptions
{
    public string Authority { get; set; } = String.Empty;
    public string ClientId { get; set; } = String.Empty;
    public string ClientSecret { get; set; } = String.Empty;
}