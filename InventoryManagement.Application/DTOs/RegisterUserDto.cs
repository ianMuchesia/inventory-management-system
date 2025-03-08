

using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Username { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }


        public string Role { get; set; } = "User";

    }
}