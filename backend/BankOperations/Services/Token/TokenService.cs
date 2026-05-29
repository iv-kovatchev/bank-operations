using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankOperations.Entities;
using BankOperations.Repositories.Token;
using Microsoft.IdentityModel.Tokens;

namespace BankOperations.Services.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var secret = _configuration["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? _configuration["Jwt:Secret"];

        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT secret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };

        foreach (var role in roles)
            claims.Add(new Claim("role", role));

        var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public async Task SaveRefreshTokenAsync(Guid userId, string token)
    {
        var expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };

        await _refreshTokenRepository.SaveAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        => await _refreshTokenRepository.GetByTokenAsync(token);

    public async Task RevokeRefreshTokenAsync(string token)
        => await _refreshTokenRepository.RevokeAsync(token);
}
