using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core.Configuration;
using IDataProtector = Microsoft.Owin.Security.DataProtection.IDataProtector;

namespace Carbon.Services.IdentityServer
{
    public class IdentityDataProtector : X509CertificateDataProtector, IDataProtector
    {
        public IdentityDataProtector(X509Certificate2 certificate) : base(certificate)
        {
        }

        public byte[] Protect(byte[] userData)
        {
            return base.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return base.Unprotect(protectedData);
        }
    }
}