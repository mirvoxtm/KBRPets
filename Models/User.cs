using System.ComponentModel.DataAnnotations;
using KBRPETS.Models.Enums;

namespace KBRPETS.Models {
    public class User {

        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(40,MinimumLength = 3)]
        public string Name { get; set; }        

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public ActiveEnum Status { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        public User () {
            Date = DateTime.UtcNow;
        }

        public User(string name, string email, string password, ActiveEnum status) {
            Name = name;
            Email = email;
            Password = password;
            Status = status;
            Date = DateTime.UtcNow;
        }
    }
}
