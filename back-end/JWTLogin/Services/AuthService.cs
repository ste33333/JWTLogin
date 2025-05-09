using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JWTLogin.Data;       
using JWTLogin.Models;    

namespace JWTLogin.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;

        private const int SaltSize = 16; 
        private const int KeySize = 32;  
        private const int Iterations = 50000; 
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;

        public AuthService(IOptions<JwtSettings> jwtSettingsOptions, ApplicationDbContext context)
        {
            _jwtSettings = jwtSettingsOptions.Value;
            _context = context;
        }

        private string HashPassword(string password, out string salt)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            salt = Convert.ToBase64String(saltBytes);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Iterations,
                _hashAlgorithmName,
                KeySize);

            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Iterations,
                _hashAlgorithmName,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromBase64String(storedHash));
        }

        public async Task<User?> RegisterAsync(RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                return null; 
            }

            var hashedPassword = HashPassword(model.Password, out var salt);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Salt = salt,
                Email = model.Email
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null) return null;
            if (!VerifyPassword(model.Password, user.PasswordHash, user.Salt)) return null;
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email) 
    };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    }