namespace KBRPETS.Models
{
    public class PetFormViewModel {
        public Pet Pet { get; set; }
        public Solicitations Solicitation { get; set; }
        public string GoogleCaptchaToken { get; set; }
    }
}