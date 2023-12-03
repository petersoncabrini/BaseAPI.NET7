using System.Security.Cryptography;
using System.Text;

namespace CustomBackend.Infra.Utils
{
    public static class CryptographyUtil
    {
        public static string ToMd5(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var result = new StringBuilder();
            var bytes = MD5.HashData(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString("X2"));

            return result.ToString();
        }
    }
}
