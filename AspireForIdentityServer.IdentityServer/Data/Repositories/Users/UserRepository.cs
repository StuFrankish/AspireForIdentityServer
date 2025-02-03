using IdentityServer.Data.DbContexts;
using IdentityServer.Data.Entities.Fido;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data.Repositories.Users;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<PublicKeyCredential>> GetUserPublicKeyCredentialsAsync(string userId)
    {
        return await _context.PublicKeyCredentials
            .AsNoTracking()
            .Where(pkc => pkc.UserId == userId)
            .ToListAsync();
    }
}
