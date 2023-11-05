using KBRPETS.Models;
using KBRPETS.Models.Enums;

namespace KBRPETS.Data {
    public class SeedingService {

        private readonly KBRPETSContext _context;

        public SeedingService(KBRPETSContext context) {
            _context = context;
        }


        // Populando o DB com entradas básicas.

        public void Seed() {
            if (_context.Pets.Any() && _context.Users.Any()) {
                return;
            }

            List<User> users = new List<User>();

            users.Add( new User()
            {
                Email = "admin@kbrtec.com.br",
                Name = "Admin",
                Password = "Admin@123",
                Status = ActiveEnum.ATIVO,
            });

            users.Add( new User()
            {
                Email = "testing@kbrtec.com.br",
                Name = "Testing",
                Password = "Teste@123",
                Status = ActiveEnum.ATIVO,
            });
            
            _context.Users.AddRange(users);

            //----------------------------------------------------------------------

            List<Pet> pets = new List<Pet>();

            pets.Add(new Pet
            {
                Name = "Tini",
                Gender = GenderEnum.FEMEA,
                Species = SpeciesEnum.GATO,
                Port = PortEnum.MEDIO,
                Race = "American Shorthair",
                Weight = 5,
                Age = 3,
                Images = string.Join(",", new List<string> {"img/tini.jpg", "img/tini-2.jpg", "img/tini-3.jpg", "img/tini-4.jpg", "img/tini-5.jpg"}),
                Location = string.Join(",", new List<string> {"Petz Bom Retiro", "Curitiba", "PR"}),
                Description = "\ud83d\udc96Frajolinha Fêmea de narizinho rosa"
            });

            _context.Pets.AddRange(pets);
            _context.SaveChanges();
        }
    }
}
