using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace TrenerPro.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // 1. ZAWSZE wypisz link w konsoli (dla łatwych testów bez wysyłania maila)
            // Szukaj tego w oknie "Output" w Visual Studio po kliknięciu "Wyślij link"
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"DO: {email}");
            Console.WriteLine($"TEMAT: {subject}");
            Console.WriteLine($"TREŚĆ: {htmlMessage}");
            Console.WriteLine("--------------------------------------------------");

            // 2. Pobierz konfigurację SMTP z appsettings.json
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            // Jeśli nie skonfigurowano maila, zakończ tutaj (tylko logowanie w konsoli)
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderPassword))
            {
                return;
            }

            // 3. Prawdziwa wysyłka maila
            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "BeFit System"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}