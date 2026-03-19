using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

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
            var server = "sandbox.smtp.mailtrap.io";
            var port = 2525; 
            var user = "57fbb90e8127dd";
            var pass = "4ebf709156ebac";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("System BeFit", "no-reply@befit.pl"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(server, port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                
            }
        }
    }
}