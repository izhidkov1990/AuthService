using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        public UserRepository(UserDbContext context)
        {
            _context = context;
        }
        public Task AddUserAcync(User user)
        {
            _context.Users.Add(user);
            return _context.SaveChangesAsync();
        }

        public async Task<User> GetUserBySteamIdAsync(string steamId)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.SteamId == steamId);
        }
    }
}