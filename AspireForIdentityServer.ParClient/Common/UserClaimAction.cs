using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace Client.Common;

public class UserClaimAction(string claimName) : ClaimAction(claimName, ClaimValueTypes.String)
{
    private readonly string _claimName = claimName;

    public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
    {
        if (userData.TryGetProperty(_claimName, out var tokens))
        {
            var values = new List<string>();

            if (tokens.ValueKind == JsonValueKind.String)
            {
                values.Add(tokens.ToString());
            }
            else
            {
                foreach (var token in tokens.EnumerateArray())
                {
                    values.Add(token.ToString());
                }
            }

            foreach (var v in values)
            {
                Claim claim = new(_claimName, v, ValueType, issuer);
                if (!identity.HasClaim(c => c.Type == identity.RoleClaimType && c.Value == claim.Value))
                {
                    identity.AddClaim(claim);
                }
            }
        }
    }
}