using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace glms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly glms.Data.AppDbContext _db;

        public AuthController(IConfiguration config, glms.Data.AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        public class LoginModel
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                return Unauthorized();

            var user = _db.Users.FirstOrDefault(u => u.Username == login.Username);
            if (user == null)
                return Unauthorized();

            var hash = ComputeHash(login.Password);
            if (!string.Equals(hash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
                return Unauthorized();

            var jwtKey = _config["Jwt:Key"] ?? "ChangeThisSecretKeyForDevOnly";
            var jwtIssuer = _config["Jwt:Issuer"] ?? "glms";
            var jwtAudience = _config["Jwt:Audience"] ?? "glms_clients";
            var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, login.Username),
                new Claim(ClaimTypes.Name, login.Username)
            };

            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            // Ensure the key is at least 256 bits for HMAC-SHA256. If the configured key is too short,
            // derive a 256-bit key from it using SHA256 to satisfy key size requirements.
            byte[] keyBytes = Encoding.UTF8.GetBytes(jwtKey ?? string.Empty);
            if (keyBytes.Length < 32)
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty));
            }
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { access_token = tokenString, token_type = "Bearer", expires_in = expiryMinutes * 60 });
        }

        private static string ComputeHash(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
