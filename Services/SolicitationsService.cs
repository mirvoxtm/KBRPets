using KBRPETS.Data;
using KBRPETS.Models;
using KBRPETS.Models.Exceptions;

namespace KBRPETS.Services {
    public class SolicitationsService {

        private readonly KBRPETSContext _context;

        // Usado para receber as solicitações de adoção.

        public SolicitationsService(KBRPETSContext context) {
            _context = context;
        }

        // Inserindo uma solicitação
        public void Insert(Solicitations solicitation) {
            _context.Add(solicitation);
            _context.SaveChanges();
        }

        // Removendo uma solicitação
        public void Remove(Solicitations solicitation) {
            _context.Remove(solicitation);
            _context.SaveChanges();
        }

        // Encontrando todas as solicitações
        public List<Solicitations> FindAllSolicitations() {

            var selectSolicitations = from solicitation
                in _context.Solicitations
                select solicitation;

            // Usando LINQ acima por conveniência.

            return selectSolicitations.ToList();
        }

        // Verificador de CPF
        public bool CpfVerifier(string cpf) {
            
            // Removendo caracteres não numericos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            
            // Se o CPF não tiver 11 digitos
            if (cpf.Length != 11) {
                return false;
            }

            // Se todos os digitos forem igual ao primeiro
            // Ex. 11111111111 ou 222222222222
            if (cpf.All(digit => digit == cpf[0])) {
                return false;
            }

            // -----------------------------------------------------

            //Primeiro digito verificador
            int soma = 0;

            for (int i = 0; i < 9; i++) {
                soma += (int)Char.GetNumericValue(cpf[i]) * (10 - i);
            }

            int restante = soma % 11;
            int checkDigit = restante < 2 ? 0 : 11 - restante;


            if (checkDigit != (int)Char.GetNumericValue(cpf[9])) {
                return false;
            }

            // Segundo Digito Verificador
            soma = 0;
            for (int i = 0; i < 10; i++) {
                soma += (int)Char.GetNumericValue(cpf[i]) * (11 - i);
            }
            restante = soma % 11;
            int secondCheckDigit = restante < 2 ? 0 : 11 - restante;

            if (secondCheckDigit != (int)Char.GetNumericValue(cpf[10])) {
                return false;
            }

            return true;
        }
    }
}
