using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace glms.Models.Entities
{
    public class Client
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        public string Region { get; set; }
        public List<Contract>? Contracts { get; set; }
    }
}
