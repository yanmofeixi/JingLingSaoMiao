using System;
using System.Configuration;
using System.Net.Mail;

namespace PokemonGoScanner.Common
{
    public class EmailAlerter
    {
        private readonly SmtpClient _smtpServer;
        private readonly string _senderAddress = ConfigurationManager.AppSettings["SenderAddress"];
        private readonly string _password = ConfigurationManager.AppSettings["SenderPassword"];
        private readonly string _receiverEmail = ConfigurationManager.AppSettings["ReceiverEmail"];

        public EmailAlerter(SmtpClient smtpClient)
        {
            _smtpServer = smtpClient;
            _smtpServer.EnableSsl = true;
            _smtpServer.UseDefaultCredentials = false;
            _smtpServer.Port = 587;
            _smtpServer.Credentials = GetCredentialFromConfig();
        }

        public void Send(string subject, string message)
        {
            _smtpServer.Send(CreateEmail(subject, message));
        }

        private MailMessage CreateEmail(string subject, string htmlBody)
        {
            var email = new MailMessage { From = new MailAddress(_senderAddress) };
            email.To.Add(GetReceiverEmailFromConfig());
            email.Subject = subject;
            email.IsBodyHtml = true;
            email.Body = htmlBody;
            return email;
        }

        private System.Net.NetworkCredential GetCredentialFromConfig()
        {
            
            if (string.IsNullOrWhiteSpace(_senderAddress))
            {
                throw new ArgumentException($"Cannot find 'SenderAddress' in App.Config or it is not valid");
            }
            if (string.IsNullOrWhiteSpace(_password))
            {
                throw new ArgumentException($"Cannot find 'SenderPassword' in App.Config or it is not valid");
            }
            return new System.Net.NetworkCredential(_senderAddress, _password);
        }

        private string GetReceiverEmailFromConfig()
        {
            if (string.IsNullOrWhiteSpace(_receiverEmail))
            {
                throw new ArgumentException($"Cannot find 'ReceiverEmail' or it is not valid");
            }
            return _receiverEmail;
        }
    }
}
