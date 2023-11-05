using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System.ComponentModel.DataAnnotations;
using Google.Cloud.RecaptchaEnterprise.V1;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;

namespace KBRPETS.Models {


    // [!] Classe feita com base nas recomendações do Brevo,
    // Muito desse código é boilerplate e pode ser encontrado
    // na respectiva documentação

    public class Email
    {
        [BindProperty, Required] 
        public string Username { get; set; }

        [BindProperty, Required, EmailAddress] 
        public string EmailAdress { get; set; }

        [BindProperty, Required] 
        public string Subject { get; set; }

        [BindProperty, Required] 
        public string Message { get; set; }


        // Por propósitos de segurança, o Brevo desativa automáticamente a API quando
        // a mesma é detectada no Github. A chave API foi enviada junto ao E-Mail
        // para o processo seletivo.

        // Se preferir, use o https://app.brevo.com/ para criar uma nova chave API em SMTP e API
        // Após isso, cole a API-KEY aqui:
        private readonly string apiKey = "API-KEY";

        public Email() {
        }

        public Email(string username, string emailAdress, string subject, string message) {
            Username = username;
            EmailAdress = emailAdress;
            Subject = subject;
            Message = message;
        }

        public void OnGet() {
        }

        string succ = "";
        string err = "";

        public void OnPost() {
   


        Configuration.Default.ApiKey["api-key"] = apiKey;

            var apiInstance = new TransactionalEmailsApi();
            string SenderName = "KBRPets";
            string SenderEmail = "kbrpets@kbrtec.com";
            SendSmtpEmailSender emailSender = new SendSmtpEmailSender(SenderName, SenderEmail);
            
            SendSmtpEmailTo emailReceiver1 = new SendSmtpEmailTo(EmailAdress, Username);
            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            To.Add(emailReceiver1);

            string HtmlContent = null;
            string textContent = Message;

            try {
                var sendSmtpEmail = new SendSmtpEmail(emailSender, To, null, null, HtmlContent, textContent, Subject);

                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                succ = "Email enviado!";
                Console.WriteLine("Response:\n" + result.ToJson());
                Console.WriteLine(succ);
            }

            catch (Exception e) {
                Console.WriteLine(err + e.Message);
            }
        }
    }
}
