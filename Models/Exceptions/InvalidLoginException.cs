namespace KBRPETS.Models.Exceptions {
    public class InvalidLoginException : ApplicationException {
        public InvalidLoginException(string message) : base(message) {

        }
    }
}