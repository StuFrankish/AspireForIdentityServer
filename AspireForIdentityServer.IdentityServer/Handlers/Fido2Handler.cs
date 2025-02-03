using Fido2NetLib;
using Fido2NetLib.Objects;
using IdentityServer.Data.DbContexts;
using IdentityServer.Data.Entities.Fido;
using IdentityServer.Data.Entities.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AuthenticatorTransport = IdentityServer.Data.Entities.Fido.AuthenticatorTransport;

namespace IdentityServer.Handlers;

public static class Fido2Handler
{
    public class CreateAttestationOptionsInputModel
    {
        public string AttestationType { get; set; } = "none";
        public AuthenticatorAttachment AuthenticatorAttachment { get; set; }
        public ResidentKeyRequirement ResidentKey { get; set; } = ResidentKeyRequirement.Discouraged;
        public UserVerificationRequirement UserVerification { get; set; } = UserVerificationRequirement.Preferred;
    }

    public static async Task<Ok<CredentialCreateOptions>> CreateAttestationOptionsAsync(
        IFido2 fido2,
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager)
    {
        CreateAttestationOptionsInputModel input = new();
        var appUser = await userManager.FindByIdAsync(httpContext.User.FindFirst("sub").Value);

        var user = new Fido2User
        {
            Name = appUser.UserName,
            Id = Guid.Parse(appUser.Id).ToByteArray(),
            DisplayName = appUser.UserName
        };

        var authenticatorSelection = new AuthenticatorSelection
        {
            AuthenticatorAttachment = input.AuthenticatorAttachment,
            ResidentKey = input.ResidentKey,
            UserVerification = input.UserVerification
        };

        var attestationPreference = input.AttestationType.ToEnum<AttestationConveyancePreference>();

        var extensions = new AuthenticationExtensionsClientInputs
        {
            Extensions = true,
            UserVerificationMethod = true,
            CredProps = true
        };

        var options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = user,
            ExcludeCredentials = [],
            AuthenticatorSelection = authenticatorSelection,
            AttestationPreference = attestationPreference,
            Extensions = extensions
        });

        httpContext.Session.SetString("Fido2AttestationOptions", options.ToJson());

        return TypedResults.Ok(options);
    }

    public static async Task<Results<BadRequest, Ok<RegisteredPublicKeyCredential>>> CreateAttestationAsync(
        [FromBody] AuthenticatorAttestationRawResponse attestationResponse,
        IFido2 fido2,
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken = default)
    {
        var json = httpContext.Session.GetString("Fido2AttestationOptions");

        if (string.IsNullOrEmpty(json))
        {
            return TypedResults.BadRequest();
        }

        var options = CredentialCreateOptions.FromJson(json);

        var credentialResult = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (@params, cancellationToken) =>
                await userManager.Users
                    .SelectMany(user => user.PublicKeyCredentials)
                    .AllAsync(credential => credential.Id != @params.CredentialId, cancellationToken)
        },
            cancellationToken) ?? throw new Exception("CredentialResult was null -- fix this");


        var credential = new PublicKeyCredential
        {
            Id = credentialResult.Id,
            PublicKey = credentialResult.PublicKey,
            SignatureCounter = credentialResult.SignCount,
            IsBackupEligible = credentialResult.IsBackupEligible,
            IsBackedUp = credentialResult.IsBackedUp,
            AttestationObject = credentialResult.AttestationObject,
            AttestationClientDataJson = credentialResult.AttestationClientDataJson,
            AttestationFormat = credentialResult.AttestationFormat,
            Aaguid = credentialResult.AaGuid,
            UserId = new Guid(credentialResult.User.Id).ToString(),
            CredentialName = "FIDO2 Provider",
            CredentialCreatedDate = DateTime.Now,
            CredentialLastUsedDate = null
        };

        foreach (var authenticatorTransport in credentialResult.Transports)
        {
            credential.AuthenticatorTransports.Add(new AuthenticatorTransport
            {
                PublicKeyCredentialId = credentialResult.Id,
                Value = authenticatorTransport
            });
        }

        if (credentialResult.PublicKey is not null)
        {
            credential.DevicePublicKeys.Add(new DevicePublicKey
            {
                PublicKeyCredentialId = credentialResult.Id,
                Value = credentialResult.PublicKey
            });
        }

        var user = await userManager.FindByIdAsync(new Guid(credentialResult.User.Id).ToString());
        user.PublicKeyCredentials.Add(credential);

        var identityResult = await userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
        {
            throw new Exception(identityResult.ToString());
        }

        httpContext.Session.Remove("Fido2AttestationOptions");

        return TypedResults.Ok(credentialResult);
    }

    public class CreateAssertionOptionsInputModel
    {
        public UserVerificationRequirement UserVerification { get; set; } = UserVerificationRequirement.Preferred;
    }

    public static Ok<AssertionOptions> CreateAssertionOptions(
        IFido2 fido2,
        HttpContext httpContext)
    {
        var input = new CreateAssertionOptionsInputModel();

        var extensions = new AuthenticationExtensionsClientInputs
        {
            Extensions = true,
            UserVerificationMethod = true
        };

        var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = [],
            UserVerification = input.UserVerification,
            Extensions = extensions
        });

        httpContext.Session.SetString("Fido2AssertionOptions", options.ToJson());

        return TypedResults.Ok(options);
    }

    public static async Task<Results<BadRequest, Ok<VerifyAssertionResult>>> CreateAssertionAsync(
        ApplicationDbContext applicationDbContext,
        [FromBody] AuthenticatorAssertionRawResponse assertionResponse,
        IFido2 fido2,
        HttpContext httpContext,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken = default)
    {
        var json = httpContext.Session.GetString("Fido2AssertionOptions");

        if (string.IsNullOrEmpty(json))
        {
            return TypedResults.BadRequest();
        }

        var options = AssertionOptions.FromJson(json);

        var credential = await userManager.Users
            .SelectMany(user => user.PublicKeyCredentials)
            .Include(credential => credential.DevicePublicKeys)
            .SingleOrDefaultAsync(credential => credential.Id == assertionResponse.Id, cancellationToken);

        if (credential is null)
        {
            return TypedResults.BadRequest();
        }

        var user = await userManager.FindByIdAsync(credential.UserId);

        if (user is null)
        {
            return TypedResults.BadRequest();
        }

        var assertionResult = await fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertionResponse,
            OriginalOptions = options,
            StoredPublicKey = credential.PublicKey,
            StoredSignatureCounter = credential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (@params, cancellationToken) =>
                await userManager.Users
                    .Where(user => user.Id == new Guid(@params.UserHandle).ToString())
                    .SelectMany(user => user.PublicKeyCredentials)
                    .AnyAsync(credential => credential.Id == @params.CredentialId, cancellationToken)
        },
            cancellationToken);

        credential.SignatureCounter = assertionResult.SignCount;
        credential.CredentialLastUsedDate = DateTime.Now;

        applicationDbContext.PublicKeyCredentials.Update(credential);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            // Set the AMR claim to indicate FIDO2/WebAuthn authentication.
            // You can use "fido", "webauthn", or "passkey" – choose the value that best
            // describes your authentication method and fits with your relying parties' expectations.
            new("amr", "fido")
        };

        await signInManager.SignInWithClaimsAsync(user, false, claims);
        httpContext.Session.Remove("Fido2AssertionOptions");

        return TypedResults.Ok(assertionResult);
    }

}
