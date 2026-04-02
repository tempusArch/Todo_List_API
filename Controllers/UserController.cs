using ToDoListAPI.models;
using ToDoListAPI.Services;
using ToDoListAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ToDoListAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase {
    private readonly UserService _userService;
    private readonly ToDoListAPIDbContext _context;

    public UserController(UserService userService, ToDoListAPIDbContext context) {
        _userService = userService;
        _context = context;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterUserResponseDto>> ERegisterUser(UserModel um) {
        var registeredUser = await _userService.RegisterUser(um);

        var result = new RegisterUserResponseDto {
            Nickname = um.Nickname,
            Email = um.Email
        };

        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserModel>> ELoginUser(LoginModel lm) {
        var loginedUser = await _userService.LoginUser(lm);

        if (loginedUser == null)
            return Unauthorized();

        var accessToken = _userService.Generate_JWT_Token(loginedUser);
        var refreshToken = _userService.Generate_RefreshToken(loginedUser.Id.ToString());

        _context.RefreshTokenTable.Add(refreshToken);
        await _context.SaveChangesAsync();

        Response.Cookies.Append("RefreshToken", refreshToken.Token, new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt
        });

        return Ok(new {Token = accessToken});
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<UserModel>> ERefresh() {
        var valueOfRefreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(valueOfRefreshToken))
            return Unauthorized();

        var kyuuRefreshToken = await _context.RefreshTokenTable.SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken);

        if (kyuuRefreshToken == null || !kyuuRefreshToken.IfActive)
            return Unauthorized();

        kyuuRefreshToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = _userService.Generate_RefreshToken(kyuuRefreshToken.UserId);
        _context.RefreshTokenTable.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var um = await _context.UserTable.SingleOrDefaultAsync(n => n.Id == int.Parse(kyuuRefreshToken.UserId));
        var newAccessToken = _userService.Generate_JWT_Token(um);

        Response.Cookies.Append("RefreshToken", newRefreshToken.Token, new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = newRefreshToken.ExpiresAt
        });

        return Ok(new { Token = newAccessToken });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<UserModel>> ELogout() {
        var valueOfRefreshToken = Request.Cookies["RefreshToken"];

        if (!string.IsNullOrEmpty(valueOfRefreshToken)) {
            var kyuuRefreshToken = await _context.RefreshTokenTable.SingleOrDefaultAsync(n => n.Token == valueOfRefreshToken);
            if (kyuuRefreshToken != null) {
                kyuuRefreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("RefreshToken");
        return NoContent();
    }

}