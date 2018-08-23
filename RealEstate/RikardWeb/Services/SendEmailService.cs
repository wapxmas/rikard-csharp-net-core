using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Data;
using RikardLib.AspLog;
using RikardLib.Parallel;
using Microsoft.Extensions.Options;
using RikardWeb.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace RikardWeb.Services
{
    public class SendEmailService : ISendEmailService
    {
        private readonly IAspLogger logger;
        private readonly IOptions<InfoOptions> infoOptions;

        private ParallelGatherSingle<EmailLetter> Worker { get; set; }

        public SendEmailService(IAspLogger logger, IOptions<InfoOptions> infoOptions)
        {
            Worker = new ParallelGatherSingle<EmailLetter>(DoSendEmail);

            this.logger = logger;
            this.infoOptions = infoOptions;

            logger.Info("SendEmailService is started");
        }

        private void DoSendEmail(EmailLetter letter)
        {
            logger.Info("Sending a email message.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Рикард-Недвижимость", infoOptions.Value.Email.From));
            message.To.Add(new MailboxAddress(letter.To));
            message.Subject = letter.Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = letter.Body;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(infoOptions.Value.Email.SmtpAddress, infoOptions.Value.Email.SmtpPort, infoOptions.Value.Email.EnableSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(infoOptions.Value.Email.Username, infoOptions.Value.Email.Password);
                client.Send(message);
                client.Disconnect(true);
            }

            logger.Info("A email message have been sent via smtp");
        }

        public void Send(EmailLetter letter) => Worker.AddData(letter);
    }
}
