using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWTLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class DataController : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult GetMyData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var username = User.FindFirstValue(ClaimTypes.Name); 

            if (userId == null)
            {
                return Unauthorized(); 
            }

            return Ok(new
            {
                message = $"Ciao {username} (ID: {userId}), questi sono dati protetti per te!",
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("public-info")] 
        [AllowAnonymous] 
        public IActionResult GetPublicInfo()
        {
            return Ok(new { info = "Queste sono informazioni pubbliche accessibili a tutti." });
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")] //esempio admin
        public IActionResult GetAdminData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(new { message = $"Dati riservati per Admin (ID: {userId})" });
        }
    }
}