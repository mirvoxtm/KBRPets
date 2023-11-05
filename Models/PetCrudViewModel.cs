using System.ComponentModel.DataAnnotations;
using KBRPETS.Models.Enums;

namespace KBRPETS.Models {
    public class PetCrudViewModel {
        public User? ThisUser { get; set; }
        public Pet Pet { get; set; }
        [Required]
        public List<IFormFile> ImageFileList { get; set; } = new List<IFormFile>(5);
        public string? ImageError { get; set; }

        // Um Failsafe para adicionar imagens nulas na lista
        // Caso não seja atingida a cota de envio.
        public PetCrudViewModel() {
            ImageFileList = new List<IFormFile>();
            for (int i = 0; i < 5; i++) {
                ImageFileList.Add(null);
            }
        }
    }
}
