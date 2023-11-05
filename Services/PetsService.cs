using KBRPETS.Data;
using KBRPETS.Models;

namespace KBRPETS.Services {
    public class PetsService {
        private readonly KBRPETSContext _context;

        public PetsService(KBRPETSContext context) {
            _context = context;
        }

        // [Aqui serão guardados os métodos com conexão ao banco]


        // Encontrando todos os Pets
        public IEnumerable<Pet> FindAllPets() {

            var selectPets =  from pet in _context.Pets select pet;
            return selectPets.ToList();
        }

        // Encontrando o Pet por seu ID
        public Pet FindById(int id) {
            return _context.Pets.FirstOrDefault(pet => pet.Id == id);
        }

        // Inserindo um Pet no DB
        public void Insert(Pet pet) {
            _context.Add(pet);
            _context.SaveChanges();
        }


        // Atualizando uma entrada de Pet
        public void Update(Pet pet) {
            _context.Update(pet);
            _context.SaveChanges();
        }

        // Removendo uma entrada de Pet
        public void Remove(Pet pet) {
            _context.Remove(pet);
            _context.SaveChanges();
        }

    }
}