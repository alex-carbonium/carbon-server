using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Carbon.Tools.OAuth
{
    public class TokenCache
    {
        private const string FileName = "token.dat";

        public string GetToken()
        {
            if (!File.Exists(FileName))
            {
                return null;
            }

            var contents = File.ReadAllBytes(FileName);
            var bytes = ProtectedData.Unprotect(contents, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }

        public void SaveToken(string token)
        {
            var contents = Encoding.UTF8.GetBytes(token);
            var bytes = ProtectedData.Protect(contents, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(FileName, bytes);
        }

        public void Clear()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}
