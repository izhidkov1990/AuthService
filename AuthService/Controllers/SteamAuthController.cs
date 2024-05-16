using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using AuthService.Services;

[ApiController]
[Route("api/[controller]")]
public class SteamAuthController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ISteamAuthService _steamAuthService;
    private readonly HttpClient _httpClient;

    public SteamAuthController(UserDbContext context, IConfiguration configuration, HttpClient httpClient, ISteamAuthService steamAuthService)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
        _steamAuthService = steamAuthService;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/api/steamauth/callback" }, "Steam");
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return Unauthorized();

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims.ToDictionary(c => c.Type, c => c.Value);
        var openIdClaim = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
        var steamId = openIdClaim.Split('/').Last();
        // Установка SteamId в куки
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("CookiesExpirationTime"))
        };
        Response.Cookies.Append("steamid", steamId, cookieOptions);
        var (userDto, token) = await _steamAuthService.ProcessSteamCallback(result.Principal);
        return Ok(new { User = userDto, Token = token });
    }
}
