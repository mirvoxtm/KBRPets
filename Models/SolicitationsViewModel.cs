namespace KBRPETS.Models {
    public class SolicitationsViewModel {
        public User? ThisUser { get; set; }
        public List<Solicitations> Solicitations { get; set; }
        public Dictionary<Solicitations, string> PetNames { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
} 
