// ImageEcryptor.cs
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LockSnap
{
    public class ImageEncryptor
    {
        private readonly string password;

        public ImageEncryptor(string password)
        {
            this.password = password;
        }

        // Encrypts the image file with a password
        public void EncryptImage(string imagePath, string encryptedPath)
        {
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SALT_VALUE"), 100000, HashAlgorithmName.SHA256); // Updated constructor
                aes.Key = key.GetBytes(32); // 256-bit key
                aes.IV = key.GetBytes(16);  // 128-bit IV

                using (FileStream fsEncrypt = new FileStream(encryptedPath, FileMode.Create))
                using (CryptoStream csEncrypt = new CryptoStream(fsEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (FileStream fsInput = new FileStream(imagePath, FileMode.Open))
                {
                    fsInput.CopyTo(csEncrypt);
                }
            }
        }

        // Decrypts the image and displays it in the app
        public BitmapImage DecryptImage(string encryptedPath)
        {
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SALT_VALUE"), 100000, HashAlgorithmName.SHA256); // Updated constructor
                aes.Key = key.GetBytes(32); // 256-bit key
                aes.IV = key.GetBytes(16);  // 128-bit IV

                using (MemoryStream msDecrypt = new MemoryStream())
                using (FileStream fsEncrypt = new FileStream(encryptedPath, FileMode.Open))
                using (CryptoStream csDecrypt = new CryptoStream(fsEncrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    csDecrypt.CopyTo(msDecrypt);
                    msDecrypt.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(msDecrypt.AsRandomAccessStream());
                    return bitmap;
                }
            }
        }
    }
}