
using Newtonsoft.Json;

namespace AuthService.DTOs
{
    public class UserDto
    {
        [JsonProperty("personaname")]
        public string Username { get; set; }
        public string Role { get; set; }
        [JsonProperty("avatarfull")]
        public string AvatarUrl { get; set; }
    }
}
