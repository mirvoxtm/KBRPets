using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using KBRPETS.Data;
using KBRPETS.Models;
using KBRPETS.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KBRPETS.Services
{
    public class UsersService {

        private readonly KBRPETSContext _context;

        // Criando a comunicacao dos Usuarios com
        // o DB. 

        public UsersService(KBRPETSContext context)
        {
            _context = context;
        }

        // Atualizando
        public void Update(User user) {
            _context.Update(user);
            _context.SaveChanges();
        }

        // Inserindo
        public void Insert(User user)
        {
             _context.AddAsync(user);
             _context.SaveChanges();
        }

        // Removendo
        public void Remove(User user) {
            _context.Remove(user);
            _context.SaveChanges();
        }

        // Encontrando todos
        public List<User> FindAllUsers() {
            return _context.Users.ToList();
        }


        // Encontrando por ID
        public User FindById(int id)
        {
            return _context.Users.FirstOrDefault(user => user.Id == id);
        }

        // Encontrando por E-mail
        // Usado para recuperar a senha por E-Mail
        public User FindByEmail(string email)
        {
            return _context.Users.FirstOrDefault(user => user.Email == email);
        }

        // Método para verificação de Login
        // Na página de Administração.
        public bool LoginVerification(User user) {
            // Compare os dados recebidos com o Banco de Dados
            var checkUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

            // Se não existe, retorna falso
            if (checkUser == null) {
                return false;
            }

            // Caso email exista, mas a senha esteja incorreta, retorne falso.
            if (user.Email != checkUser.Email || user.Password != checkUser.Password) {
                return false;
            }

            // Caso contrário, retorne verdadeiro.
            return true;
        }


        // Método auxiliar para retornar o ID do usuário
        // Que for logado para utilizar durante a execução do painel
        public int AssociateId(User user) {
            var foundUser = 
                _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            return foundUser.Id;
        }
    }
}