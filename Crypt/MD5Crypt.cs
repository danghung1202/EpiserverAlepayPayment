using System.Security.Cryptography;
using System.Text;

namespace Foundation.Commerce.Payment.Alepay
{
    public static class MD5Crypt
    {
        public static string MD5Hash(string input)
        {
            var hash = new StringBuilder();
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] bytes = md5Provider.ComputeHash(new UTF8Encoding().GetBytes(input));
                
                foreach (var t in bytes)
                {
                    hash.Append(t.ToString("x2"));
                }
                return hash.ToString();
            }
        }
    }
}