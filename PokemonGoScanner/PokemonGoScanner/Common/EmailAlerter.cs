﻿namespace PokemonGoScanner.Common
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

        public void Send(string subject, string message, string emailForReceiving)
        {
            _smtpServer.Send(CreateEmail(subject, message, emailForReceiving));
        }

        private MailMessage CreateEmail(string subject, string htmlBody, string emailForReceiving)
        {
            var email = new MailMessage { From = new MailAddress(_senderAddress) };
            email.Subject = subject;
            email.IsBodyHtml = true;
            email.Body = htmlBody;
            email.To.Add(emailForReceiving);
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
