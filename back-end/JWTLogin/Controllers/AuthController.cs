using JWTLogin.Models;
using JWTLogin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using JWTLogin.Models;    
using JWTLogin.Services;  

namespace JWTLogin.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) // AuthService iniettato qui
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authService.RegisterAsync(model);

            if (user == null)
            {
                return BadRequest(new { message = "L'username è già in uso." });
            }
            return Ok(new { message = "Registrazione avvenuta con successo.", userId = user.Id, username = user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _authService.LoginAsync(model);

            if (token == null)
            {
                return Unauthorized(new { message = "Credenziali non valide." });
            }

            return Ok(new { token = token });
        }
    }
}