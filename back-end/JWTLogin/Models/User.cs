using System.ComponentModel.DataAnnotations;

namespace JWTLogin.Models
{
    public class User
    {
        public int Id { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Salt { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
