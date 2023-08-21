using System.Security.Cryptography;
using System.Text;

namespace protecno.api.sync.domain.helpers
{
    public static class HashHelper
    {
        public static string CreateHash<T>(T reportRQ) where T : class
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(reportRQ.ToString()));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }
}
