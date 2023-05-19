using System;
using System.Security.Cryptography;
using System.Text;

namespace Pawn.Libraries
{
    public class Encryption
    {
        private string _key { get; set; }
        public Encryption() { }
        public Encryption(string key)
        {
            _key = key;
        }
        /// <summary>
        /// Mã hóa chuỗi bất kì Ex: new Util(key).EncryptString(value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string EncryptString(string value)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();
            var hashProvider = new MD5CryptoServiceProvider();
            var tdesKey = hashProvider.ComputeHash(utf8.GetBytes(_key));

            var tdesAlgorithm = new TripleDESCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var dataToEncrypt = utf8.GetBytes(value);

            try
            {
                var encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }
            return Convert.ToBase64String(results);
        }
        /// <summary>
        ///  Giải mã chuỗi đã mã hóa Ex: new Util(key).DecryptString(value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string DecryptString(string value)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();

            var hashProvider = new MD5CryptoServiceProvider();
            var tdesKey = hashProvider.ComputeHash(utf8.GetBytes(_key));

            var tdesAlgorithm = new TripleDESCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var dataToDecrypt = Convert.FromBase64String(value);

            try
            {
                var decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }
            return utf8.GetString(results);
        }

        /// <summary>
        /// Hàm mã hóa MD5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Md5Encryption(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(value));
            var stringBuilder = new StringBuilder();
            foreach (byte num in hash)
                stringBuilder.Append(num.ToString("x2"));
            return stringBuilder.ToString();
        }
    }
}
