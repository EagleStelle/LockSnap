// EncryptionHandler.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LockSnap
{
    internal class EncryptionHandler
    {
        private ImageEncryptor imageEncryptor;

        public EncryptionHandler(string password)
        {
            imageEncryptor = new ImageEncryptor(password);
        }

        // Encrypts all images in the folder or a single image
        public void EncryptImagesInFolder(string folderPath)
        {
            var imageFiles = Directory.GetFiles(folderPath, "*.*")
                                      .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"));

            foreach (var imagePath in imageFiles)
            {
                string encryptedPath = imagePath + ".enc";
                imageEncryptor.EncryptImage(imagePath, encryptedPath);
            }
        }

        public void EncryptSingleImage(string imagePath)
        {
            string encryptedPath = imagePath + ".enc";
            imageEncryptor.EncryptImage(imagePath, encryptedPath);
        }

        // Decrypts all encrypted images in the folder or a single encrypted file
        public Dictionary<string, BitmapImage> DecryptImagesInFolder(string folderPath)
        {
            var decryptedImages = new Dictionary<string, BitmapImage>();
            var encryptedFiles = Directory.GetFiles(folderPath, "*.enc");

            foreach (var encryptedPath in encryptedFiles)
            {
                decryptedImages[Path.GetFileName(encryptedPath)] = imageEncryptor.DecryptImage(encryptedPath);
            }

            return decryptedImages;
        }

        public BitmapImage DecryptSingleImage(string encryptedPath)
        {
            return imageEncryptor.DecryptImage(encryptedPath);
        }
    }
}
