using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserBySteamIdAsync(string steamId);
        Task AddUserAcync(User user);
    }
}