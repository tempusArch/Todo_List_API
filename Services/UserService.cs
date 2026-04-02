using ToDoListAPI.Data;
using ToDoListAPI.models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace ToDoListAPI.Services;

public class UserService {
    private readonly ToDoListAPIDbContext _context;
    private readonly IConfiguration _config;
    public UserService (ToDoListAPIDbContext context, IConfiguration config) {
        _context = context;
        _config = config;
    }

    public async Task<UserModel> RegisterUser(UserModel um) {
        um.PasswordHash = BCrypt.Net.BCrypt.HashPassword(um.PasswordHash);

        _context.UserTable.Add(um);
        await _context.SaveChangesAsync();

        return um; 
    }

    public async Task<UserModel> LoginUser(LoginModel lm) {
        var user = await _context.UserTable.SingleOrDefaultAsync(n => n.Email == lm.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(lm.Password, user.PasswordHash))
            return null;

        return user;
    }

    public string Generate_JWT_Token(UserModel um) {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, um.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, um.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JWT:Issuer"],
            audience: _config["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenModel Generate_RefreshToken(string UserId) {
        return new RefreshTokenModel {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };
    }
}