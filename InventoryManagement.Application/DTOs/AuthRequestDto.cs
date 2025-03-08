using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs
{
    public class AuthRequestDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

       
        public string Password { get; set; }
    }
}