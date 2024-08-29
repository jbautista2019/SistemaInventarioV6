using Mailjet.Client;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Utilidades
{
    public class EmailSender : IEmailSender
    {
        //public string MailJetSecret { get; set; }
        private readonly SmtpClient _smtpClient;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        public EmailSender(IConfiguration _config)
        {
           
            var emailSettings = _config.GetSection("EmailSettings");
            _smtpClient = new SmtpClient(emailSettings["SmtpServer"])
            {
                Port = int.Parse(emailSettings["SmtpPort"]),
                Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]),
                EnableSsl = true // O false, dependiendo de tu configuración
            };
            _smtpUsername = emailSettings["SmtpUsername"];
            _smtpPassword = emailSettings["SmtpPassword"];
        }
        public  Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

             return   _smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Manejo del error específico de SMTP
                Console.WriteLine($"SMTP Error: {ex.Message}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Manejo de otros errores
                Console.WriteLine($"Error: {ex.Message}");
                return Task.CompletedTask;
            }
        }
        //public  Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    var client = new MailjetClient(MailJetSecret);
        //    var from = new MailAddress("jacklinbautista@gmail.com");

        //    MailMessage msg = new MailMessage();
        //    msg.To.Add(new MailAddress(email));
        //    msg.From = from;
        //    msg.Subject = subject;
        //    msg.Body = htmlMessage;
        //    var smtpClient = new SmtpClient();
        //    smtpClient.Credentials = new NetworkCredential("jacklinbautista@gmail.com", "1549ed479bd5cd5731c714c3ea71dbd0");
        //    smtpClient.Send(msg);
        //    return smtpClient.SendMailAsync(msg);
        //}
    }
}
