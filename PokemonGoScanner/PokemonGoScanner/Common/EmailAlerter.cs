namespace PokemonGoScanner.Common
{
    using System;
    using System.Data.Entity;
    using System.Net.Mail;
    using Microsoft.Azure;
    using AppModels;
    using System.Threading.Tasks;
    using System.Linq;
    public class EmailAlerter
    {
        private readonly SmtpClient _smtpServer;
        private readonly string _senderAddress;
        private readonly string _password;
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        public EmailAlerter(SmtpClient smtpClient)
        {
            this._senderAddress = CloudConfigurationManager.GetSetting("SenderAddress");
            this._password = CloudConfigurationManager.GetSetting("SenderPassword");
            _smtpServer = smtpClient;
            _smtpServer.EnableSsl = true;
            _smtpServer.UseDefaultCredentials = false;
            _smtpServer.Port = 587;
            _smtpServer.Credentials = GetCredentialFromConfig();
        }

        public async Task SendAsync(string subject, string message, Location location)
        {
            _smtpServer.Send(await CreateEmailAsync(subject, message, location));
        }

        private async Task<MailMessage> CreateEmailAsync(string subject, string htmlBody, Location location)
        {
            var email = new MailMessage { From = new MailAddress(_senderAddress) };
            var locationSubscriptions = await db.LocationSubscriptions.Include(s => s.User).Where(l => l.LocationId == location.Id).ToListAsync();
            email.Subject = subject;
            email.IsBodyHtml = true;
            email.Body = htmlBody;
            email.To = string.Join(",", locationSubscriptions.Select(l => l.User.EmailForAlert).ToList());
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
    }
}
