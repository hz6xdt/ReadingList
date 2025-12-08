using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReadingList.Exceptions;
using ReadingList.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReadingList.Controllers;

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
        var result = await mgr.PasswordSignInAsync(creds.Username, creds.Password, false, false);

        return result.Succeeded ? (IActionResult)Ok() : throw new AuthException("Invalid username or password.");
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await mgr.SignOutAsync();
        return Ok(new
        {
            success = true
        });
    }

    [HttpPost("token", Name = "Token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResponse))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Token([FromBody] Credentials creds)
    {
        _ = await mgr.PasswordSignInAsync(creds.Username, creds.Password, false, false);

        IdentityUser? user = await usermgr.FindByNameAsync(creds.Username) ?? throw new AuthException("Invalid username or password.");

        if ((await mgr.CheckPasswordSignInAsync(user, creds.Password, true)).Succeeded)
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
        else
        {
            throw new AuthException("Invalid username or password.");
        }
    }

    [HttpPost("register", Name = "Register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiErrorResponse))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest registerRequest)
    {
        ArgumentNullException.ThrowIfNull(registerRequest);

        IdentityUser? user = await usermgr.FindByNameAsync(registerRequest.Username);

        if (user != null)
        {
            throw new AuthException("That username is not available.");
        }

        user = await usermgr.FindByEmailAsync(registerRequest.Email);

        if (user != null)
        {
            throw new AuthException("That email address is already in use.");
        }

        user = new IdentityUser
        {
            UserName = registerRequest.Username,
            Email = registerRequest.Email
        };

        var result = await usermgr.CreateAsync(user, registerRequest.Password);
        if (result.Succeeded)
        {
            return Ok();
        }
        else
        {
            List<string> errors = [];

            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }
            throw new AuthException("Invalid registration request.", errors);
        }
    }

    public class Credentials
    {
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [StringLength(256, MinimumLength = 12)]
        public string Password { get; set; } = string.Empty;
    }
}
