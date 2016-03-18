using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.SPAM
{
    public class Mail_Send
    {
        private Enums.TypeSpam _typeSpam;
        private MailAddress _maFrom;
        private bool _isBodyHtml;
        private string _ip;
        public Mail_Send(Enums.TypeSpam TS, bool isBodyHtml, string IP)
        {
            _typeSpam = TS;
            _maFrom = new MailAddress(ConfigurationManager.AppSettings["MailFrom"], ConfigurationManager.AppSettings["MailFromSubscription"], Encoding.UTF8);
            _isBodyHtml = isBodyHtml;
            _ip = IP;
        }

        public void SendMessage(ContextModels.Account currentUser, List<Attribute> Attributes)
        {




        }

        private void SendMessageRegistration()
        {




        }














        private void SendEmail(List<string> to, string messageBody, string messageSubject, List<string> Attachments)
        {
            foreach (var _to in to)
            {
                SendEmail(_to, messageBody, messageSubject, Attachments);
            }
        }

        private void SendEmail(string to, string messageBody, string messageSubject, List<string> attachments)
        {
            MailAddress maTo = new MailAddress(to);

            using (var message = new MailMessage(_maFrom, maTo))
            {
                message.Subject = messageSubject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Body = messageBody;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = false;

                if (_isBodyHtml)
                    message.IsBodyHtml = _isBodyHtml;

                if (attachments != null && attachments.Count > 0)
                    foreach (var attachment in attachments)
                    {
                        message.Attachments.Add(new Attachment(attachment));
                    }
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                using (var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpHost"], smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Send(message);
                }
            }
        }

        
    }
}

