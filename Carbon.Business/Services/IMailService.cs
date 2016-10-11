using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Business.Domain.Marketing;

namespace Carbon.Business.Services
{
    public interface IMailService
    {
        void Send(Email email, string to = null);
        void UpdateFromTemplate(Email email);
        void Format(Email email);
        void SendBulkEmail(EmailServer server, BulkEmail email, List<string> recipients, IDictionary<string, List<string>> substitutions);
    }
}