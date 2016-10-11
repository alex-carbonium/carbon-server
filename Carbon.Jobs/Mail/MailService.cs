//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using Carbon.Business;
//using Carbon.Business.Domain;
//using Carbon.Business.Domain.Marketing;
//using Carbon.Business.Services;
//using Carbon.Framework.Logging;
//using RazorEngine;
//using RazorEngine.Configuration;
//using RazorEngine.Templating;
//using SendGrid;
//using SendGrid.SmtpApi;
//using Carbon.Business.Logging;

//namespace Carbon.Data.Mail
//{
//    //TODO: fix mail service
//    public class MailService : IMailService
//    {
//        private readonly AppSettings _appSettings;
//        private readonly ILogService _logService;
//        private readonly ConcurrentDictionary<string, string> _emails = new ConcurrentDictionary<string, string>();
//        private NetworkCredential _credentials;
//        private Logger _logger;

//        public MailService(AppSettings appSettings, ILogService logService)
//        {
//            _appSettings = appSettings;
//            _logService = logService;

//            var templateConfig = new TemplateServiceConfiguration
//            {
//                Resolver = new DelegateTemplateResolver(ResolveTemplate)
//            };
//            Razor.SetTemplateService(new TemplateService(templateConfig));  
//        }

//        public string ResolveTemplate(string templateName)
//        {
//            Func<string, string> resolver = n =>
//            {
//                var folder = _appSettings.GetPhysicalPath("~/Resources/EmailTemplates");
//                var file = Path.Combine(folder, n + ".cshtml");
//                return File.ReadAllText(file);
//            };
//            return _emails.GetOrAdd(templateName, resolver);         
//        }

//        public void UpdateFromTemplate(Email email)
//        {
//            dynamic viewBag = new DynamicViewBag();
//            email.Content = Razor.Parse(ResolveTemplate(email.TemplateName), (dynamic)email.Model, viewBag, cacheName: email.TemplateName);
//            if (!string.IsNullOrEmpty(viewBag.Subject))
//            {
//                email.Subject = viewBag.Subject;
//            }            
//        }

//        public void Format(Email email)
//        {                        
//            var result = PreMailer.Net.PreMailer.MoveCssInline(email.Content, removeStyleElements: true, ignoreElements: "#mobileStyles");
//            email.Content = result.Html;
//        }

//        public void Send(Email email, string to = null)
//        {
//            if (!string.IsNullOrEmpty(email.TemplateName))
//            {
//                UpdateFromTemplate(email);
//            }
            
//            Format(email);

//            var message = new SendGridMessage();
//            if (!string.IsNullOrEmpty(email.FromAddress))
//            {
//                message.From = new MailAddress(email.FromAddress, email.FromName);
//            }
//            else
//            {
//                //message.From = new MailAddress(_appSettings.Smtp.SenderEmail, _appSettings.Smtp.SenderName);
//            }
//            message.AddTo(to ?? email.To);

//            message.Subject = email.Subject;
//            message.Html = email.Content;               

//            LogMessage(message);

////            if (_appSettings.Smtp.Enabled)
////            {
////                new Web(GetCredentials()).Deliver(message);
////            }
////            if (_appSettings.Smtp.DumpMails)
////            {
////                var dir = _appSettings.GetPhysicalPath("~/Logs/Emails");
////                Directory.CreateDirectory(dir);
////                var file = Path.Combine(dir, "Email_" + DateTime.Now.Ticks + ".html");
////                File.AppendAllText(file, "To: " + message.To);
////                File.AppendAllText(file, "<br/>Subject: " + message.Subject);
////                File.AppendAllText(file, message.Html);
////            }
//        }        

//        public void SendBulkEmail(EmailServer server, BulkEmail email, List<string> recipients, IDictionary<string, List<string>> substitutions)
//        {
//            var message = CreateBulkMailMessage(email);
//            var header = CreateBulkMailHeader(recipients, substitutions);
//            message.Headers.Add("X-SMTPAPI", header.JsonString());

//            var client = new SmtpClient
//            {                
//                Host = server.Host,
//                Port = server.Port,
//                Timeout = 20000,
//                DeliveryMethod = SmtpDeliveryMethod.Network,
//                UseDefaultCredentials = false,
//                Credentials = new NetworkCredential(server.UserName, server.Password)
//            };
//            client.Send(message);
//        }

//        private MailMessage CreateBulkMailMessage(BulkEmail email)
//        {
//            var message = new MailMessage
//            {
////                From = !string.IsNullOrEmpty(email.FromAddress)
////                    ? new MailAddress(email.FromAddress, email.FromName)
////                    : new MailAddress(_appSettings.Smtp.SenderEmail, _appSettings.Smtp.SenderName),
//                Subject = email.Subject,
//                Body = email.Content,
//                BodyEncoding = System.Text.Encoding.UTF8,
//                IsBodyHtml = true
//            };
//            if (!string.IsNullOrEmpty(email.ReplyTo))
//            {
//                message.ReplyToList.Add(email.ReplyTo);
//            }
//            message.To.Add(email.To);
//            return message;
//        }

//        private static Header CreateBulkMailHeader(List<string> recipients, IDictionary<string, List<string>> substitutions)
//        {
//            var header = new Header();
//            header.EnableFilter("clicktrack");
//            header.EnableFilter("dkim");
//            header.AddFilterSetting("dkim", new List<string> { "use_from" }, "1");
//            header.SetTo(recipients);
//            foreach (var substitution in substitutions)
//            {
//                header.AddSubstitution(substitution.Key, substitution.Value);
//            }
//            return header;
//        }

//        private void LogMessage(SendGridMessage message)
//        {
//            var logger = GetLogger();
//            logger.InfoWithContext("Email", contextAction: c =>
//            {
//                if (message.To.Length == 1)
//                {
//                    c["userEmail"] = message.To.Single().Address;
//                }
//                else
//                {
//                    c["recepients"] = string.Join(", ", message.To.Select(x => x.Address));
//                }

//                c["subject"] = message.Subject;
//                c["body"] = message.Html;
//            });
//        }

//        private Logger GetLogger()
//        {
//            if (_logger == null)
//            {
//                _logger = _logService.GetLogger(this);
//            }
//            return _logger;
//        }

//        private NetworkCredential GetCredentials()
//        {
//            if (_credentials == null)
//            {
//                _credentials = null;
//                //_credentials = new NetworkCredential(_appSettings.Smtp.SendGridUsername, _appSettings.Smtp.SendGridPassword);
//            }
//            return _credentials;
//        }
//    }
//}