// EncryptionHelper.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LockSnap
{
    public static class EncryptionHelper
    {
        private static readonly int Iterations = 100_000; // Increased iteration count for security

        public static byte[] Encrypt(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();
                aes.Key = DeriveKey(password, aes.KeySize / 8);
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Write IV to start of stream
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(data, iv, iv.Length);
                aes.IV = iv;
                aes.Key = DeriveKey(password, aes.KeySize / 8);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, iv.Length, data.Length - iv.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static byte[] DeriveKey(string password, int keySize)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("salt-value"), Iterations, HashAlgorithmName.SHA256))
            {
                return rfc2898.GetBytes(keySize);
            }
        }
    }
}
