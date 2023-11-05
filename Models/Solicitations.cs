using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KBRPETS.Models {
    public class Solicitations {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(40,MinimumLength = 3)]
        public string Name { get; set; }

        public int PetId { get; set; } 

        [Required]
        [StringLength(11,MinimumLength = 11)]
        public string Cpf { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(11,MinimumLength = 11)]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        public bool Active { get; set; }

        public Solicitations() {
        }

    }
}
