using System.Threading.Tasks;
using JWTLogin.Models;
using JWTLogin.Models; 

namespace JWTLogin.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterModel model); 
        Task<string?> LoginAsync(LoginModel model); 
    }
}