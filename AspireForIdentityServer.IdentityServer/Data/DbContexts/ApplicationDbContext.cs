using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityServer.Data.Entities.Fido;
using IdentityServer.Data.Entities.Identity;

namespace IdentityServer.Data.DbContexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AuthenticatorTransport> AuthenticatorTransports { get; set; }
    public DbSet<DevicePublicKey> DevicePublicKeys { get; set; }
    public DbSet<PublicKeyCredential> PublicKeyCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuthenticatorTransport>()
            .ToTable("AuthenticatorTransports", schema: "Fido")
            .HasKey(transport => new { transport.PublicKeyCredentialId, transport.Value });

        builder.Entity<DevicePublicKey>()
            .ToTable("DevicePublicKeys", schema: "Fido")
            .HasKey(key => new { key.PublicKeyCredentialId, key.Value });

        builder.Entity<PublicKeyCredential>()
            .ToTable("PublicKeyCredentials", schema: "Fido")
            .HasOne<ApplicationUser>()
            .WithMany(user => user.PublicKeyCredentials)
            .HasForeignKey(credential => credential.UserId);
    }
}