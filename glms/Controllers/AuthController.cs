using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace glms.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var resp = await client.PostAsJsonAsync("api/auth/token", new { Username = username, Password = password });
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            var obj = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (obj != null && obj.TryGetValue("access_token", out var tokenObj))
            {
                var token = tokenObj?.ToString();
                HttpContext.Session.SetString("AccessToken", token ?? string.Empty);

                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
            }

            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
            return RedirectToAction("Index", "Home");
        }
    }
}
