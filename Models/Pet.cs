using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using KBRPETS.Models.Enums;

namespace KBRPETS.Models {
    public class Pet {

        [Key]
        public int? Id { get; set; }
        
        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        [StringLength(15, MinimumLength = 3)]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public GenderEnum? Gender {get; set;}
        
        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public SpeciesEnum? Species { get; set; }
        
        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public PortEnum? Port {get; set;}
        
        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public string? Race { get; set; }


        public int? Weight { get; set; }



        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public int? Age { get; set; }


        public string? Images { get; set; }

        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public string? Location { get; set; }



        public string? Description { get; set; }


        [Required(ErrorMessage = "{0} é um campo obrigatório!")]
        public bool Active { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        // _____________________________________________________

        public Pet () {
            Date = DateTime.UtcNow;
        }

        public Pet(string name, GenderEnum gender, SpeciesEnum species, PortEnum port, string race, int weight, int age,
            string images, string location, string description, bool active) {

            Name = name;
            Gender = gender;
            Species = species;
            Port = port;
            Race = race;
            Weight = weight;
            Age = age;
            Images = images;
            Location = location;
            Description = description;
            Date = DateTime.UtcNow;
            Active = active;
        }
    }
}
