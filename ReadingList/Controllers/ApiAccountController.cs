using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
            if (result.Succeeded)
            {
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
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
            if (await CheckPassword(creds))
            {
                JwtSecurityTokenHandler handler = new();
                byte[] secret = Encoding.ASCII.GetBytes(config["JwtSecret"]!);
                SecurityTokenDescriptor descriptor = new()
                {
                    Subject = new ClaimsIdentity(new Claim[] { new(ClaimTypes.Name, creds.Username) }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
                };
                SecurityToken token = handler.CreateToken(descriptor);
                return Ok(new
                {
                    success = true,
                    token = handler.WriteToken(token)
                });
            }
            return Unauthorized();
        }

        private async Task<bool> CheckPassword(Credentials creds)
        {
            IdentityUser? user = await usermgr.FindByNameAsync(creds.Username);
            if (user != null)
            {
                return (await mgr.CheckPasswordSignInAsync(user, creds.Password, true)).Succeeded;
            }
            return false;
        }

        public class Credentials
        {
            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
        }
    }
}
