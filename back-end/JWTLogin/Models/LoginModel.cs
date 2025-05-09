using System.ComponentModel.DataAnnotations;

namespace JWTLogin.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "L'username è obbligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La password è obbligatoria")]
        public string Password { get; set; } = string.Empty;
    }
    
}
