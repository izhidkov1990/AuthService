
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Models;
using Newtonsoft.Json.Linq;

namespace AuthService.Services
{
    public interface ISteamAuthService
    {
        string GenerateJwtToken(User user);
        Task<UserDto> GetSteamUserInfo(string steamId);
        Task<(UserDto, string)> ProcessSteamCallback(ClaimsPrincipal principal);
    }
}