using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReadingList.Controllers
{
    [ApiController]
    [Route("/api/account")]
    public class ApiAccountController(SignInManager<IdentityUser> mgr, UserManager<IdentityUser> usermgr, IConfiguration config) : ControllerBase
    {
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Login([FromBody] Credentials creds)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await mgr.PasswordSignInAsync(creds.Username, creds.Password, false, false);
            return result.Succeeded ? Ok() : Unauthorized();
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await mgr.SignOutAsync();
            return Ok();
        }

        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Token([FromBody] Credentials creds)
        {
            IdentityUser? user = await usermgr.FindByNameAsync(creds.Username);
            if (user != null && (await mgr.CheckPasswordSignInAsync(user, creds.Password, true)).Succeeded)
            {
                List<Claim> claims = [new(ClaimTypes.Name, creds.Username)];

                var roles = await usermgr.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }

                byte[] secret = Encoding.ASCII.GetBytes(config["Data:JwtSecret"]!);

                JwtSecurityToken token = new(claims: claims, expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: new(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature));

                JwtSecurityTokenHandler handler = new();

                return Ok(new
                {
                    success = true,
                    token = handler.WriteToken(token)
                });
            }

            return Unauthorized();
        }

        public class Credentials
        {
            [StringLength(256)]
            public string Username { get; set; } = string.Empty;

            [StringLength(256, MinimumLength = 12)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
