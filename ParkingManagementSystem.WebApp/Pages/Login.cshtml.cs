using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ParkingManagementSystem.Contracts.User.LoginUser;

namespace ParkingManagementSystem.WebApp.Pages;

public class Login : PageModel
{
    [BindProperty] public LoginUserRequest LoginUserRequest { get; set; }

    public string? ErrorMessage { get; set; }

    private readonly IHttpClientFactory _httpClientFactory;

    public Login(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("API");
            var content = new StringContent(
                JsonSerializer.Serialize(LoginUserRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/users/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginUserResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Response.Cookies.Append("jwt_token", loginResponse!.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
                    new(ClaimTypes.GivenName, loginResponse.FirstName),
                    new(ClaimTypes.Surname, loginResponse.LastName),
                    new(ClaimTypes.Email, loginResponse.Email)
                };

                if (loginResponse.Roles.Count != 0)
                {
                    claims.AddRange(loginResponse.Roles.Select(role =>
                        new Claim(ClaimTypes.Role, role.ToString())));
                }

                var claimsIdentity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return LocalRedirect("/");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = errorContent;
                return Page();
            }
        }
        catch
        {
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }
}