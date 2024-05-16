namespace AuthService.Models
{
    public class User
{
    public int Id { get; set; }
    public string SteamId { get; set; }
    public string Username { get; set; }
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; }
}
}