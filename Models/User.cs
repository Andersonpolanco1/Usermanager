using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Usermanager.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime TokenCreatedAt { get; set; }
        public DateTime TokenExpiresAt { get; set; }
    }
}
