using Microsoft.AspNetCore.Mvc;

namespace KBRPETS.Models {
    public class PainelViewModel {
        public List<User> AllUsers { get; set; }
        public User ThisUser { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
