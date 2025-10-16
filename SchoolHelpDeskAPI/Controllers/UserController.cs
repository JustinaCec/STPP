using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolHelpDeskAPI.Data;
using SchoolHelpDeskAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolHelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SchoolHelpDeskContext _context;
        private readonly IConfiguration _configuration;

        public UserController(SchoolHelpDeskContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Prisijungia vartotoją ir generuoja JWT tokeną.
        /// </summary>
        /// <param name="request">Prisijungimo duomenys</param>
        /// <returns>JWT tokeną arba Unauthorized atsakymą</returns>
        /// <response code="200">Prisijungimas sėkmingas</response>
        /// <response code="401">Neteisingi prisijungimo duomenys</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password_Hash))
                return Unauthorized("Invalid credentials.");

            var jwt = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expiry = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new { Token = jwt, RefreshToken = refreshToken });
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("id", user.Id.ToString()),
            new Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
 private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        /// <summary>
        /// Atnaujina prieigos (JWT) žetoną naudojant atnaujinimo žetoną.
        /// </summary>
        /// <param name="request">Atnaujinimo užklausa</param>
        /// <returns>Naujas prieigos žetonas ir atnaujinimo žetonas</returns>
        /// <response code="200">Naujas prieigos žetonas sėkmingai sugeneruotas</response>
        /// <response code="401">Atnaujinimo žetonas negaliojantis arba pasibaigęs</response>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request is null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("Invalid request.");

            var tokenEntry = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.RevokedAt == null);

            if (tokenEntry == null || tokenEntry.Expiry <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired or invalid.");

            // Revoke old token
            tokenEntry.RevokedAt = DateTime.UtcNow;

            // Issue new token
            var jwt = GenerateJwtToken(tokenEntry.User!);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = tokenEntry.UserId,
                Token = newRefreshToken,
                Expiry = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new { Token = jwt, RefreshToken = newRefreshToken });
        }



        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; } = null!;
        }

        /// <summary>
        /// Registruoja naują vartotoją.
        /// </summary>
        /// <param name="request">Registracijos duomenys</param>
        /// <returns>Patvirtinimo pranešimą arba klaidą</returns>
        /// <response code="201">Vartotojas sėkmingai sukurtas</response>
        /// <response code="400">Elektroninio pašto adresas jau egzistuoja</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                Email = request.Email,
                Password_Hash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "Student",
                RefreshTokens = new List<string>(),
                RefreshTokenExpiries = new List<DateTime>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "User registered successfully" });
        }

        /// <summary>
        /// Atnaujina vartotojo duomenis pagal ID.
        /// </summary>
        /// <param name="id">Vartotojo ID</param>
        /// <param name="request">Nauji vartotojo duomenys</param>
        /// <returns>Atnaujintą vartotoją arba klaidą</returns>
        /// <response code="200">Vartotojas atnaujintas sėkmingai</response>
        /// <response code="404">Vartotojas nerastas</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password_Hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (!string.IsNullOrEmpty(request.Role))
                user.Role = request.Role;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        /// <summary>
        /// Ištrina vartotoją pagal ID.
        /// </summary>
        /// <param name="id">Vartotojo ID</param>
        /// <returns>Patvirtinimą arba klaidą</returns>
        /// <response code="204">Vartotojas sėkmingai ištrintas</response>
        /// <response code="404">Vartotojas nerastas</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Atsijungia vartotoją.
        /// </summary>
        /// <returns>Patvirtinimo pranešimą</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            var tokenEntry = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.RevokedAt == null);

            if (tokenEntry != null)
            {
                tokenEntry.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }
        }


        public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Role { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
