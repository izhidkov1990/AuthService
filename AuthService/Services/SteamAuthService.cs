

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace AuthService.Services
{
    public class SteamAuthService : ISteamAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
         private readonly IUserRepository _userRepository;
        public SteamAuthService(IConfiguration configuration, HttpClient httpClient, IUserRepository userRepository)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _userRepository = userRepository;
        }
        public string GenerateJwtToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("SteamId", user.SteamId),
                new Claim("Role", user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto> GetSteamUserInfo(string steamId)
        {
            var apiKey = _configuration["Steam:ApiKey"];
            var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={steamId}";
            var response = await _httpClient.GetStringAsync(url);
            var userInfo = JObject.Parse(response)["response"]["players"].First.ToObject<UserDto>();
            return userInfo;
        }

        public async Task<(UserDto, string)> ProcessSteamCallback(ClaimsPrincipal principal)
        {
            var claims = principal.Identities
            .FirstOrDefault()?.Claims
            .ToDictionary(c => c.Type, c => c.Value);

            var openIdClaim = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
            var steamId = openIdClaim.Split('/').Last();

            var userInfo = await GetSteamUserInfo(steamId);
            var username = userInfo.Username ?? string.Empty;
            var avatarUrl = userInfo.AvatarUrl ?? string.Empty;

            var user = await _userRepository.GetUserBySteamIdAsync(steamId);
            if (user == null)
            {
                user = new User
                {
                    SteamId = steamId,
                    Username = username,
                    Role = "user",
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAcync(user);
            }
            var token = GenerateJwtToken(user);
            var userDto = new UserDto
            {
                Username = user.Username,
                Role = user.Role,
                AvatarUrl = avatarUrl
            };
            return (userDto, token);
        }
    }
}