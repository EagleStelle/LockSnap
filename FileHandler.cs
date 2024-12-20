﻿// FileHandler.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace LockSnap
{
    internal class FileHandler
    {
        private readonly XamlRoot _xamlRoot;
        private readonly DialogHandler _dialogHandler;

        // Holds file references instead of fully loaded images, allowing on-demand loading
        private List<StorageFile> _imageFiles = new List<StorageFile>();
        private Dictionary<int, BitmapImage> _imageCache = new Dictionary<int, BitmapImage>();
        private const int CacheSize = 3; // Number of images to keep in memory

        public string DecryptionPassword { get; private set; }

        public FileHandler(XamlRoot xamlRoot, DialogHandler dialogHandler)
        {
            _xamlRoot = xamlRoot;
            _dialogHandler = dialogHandler;
        }

        public async Task<List<StorageFile>> SelectMultipleImagesAsync(IntPtr hwnd, XamlRoot xamlRoot)
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".enc");

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            var files = await openPicker.PickMultipleFilesAsync();

            _imageFiles.Clear();
            var encryptedFiles = new List<StorageFile>();

            foreach (var file in files)
            {
                if (file.FileType == ".enc")
                {
                    encryptedFiles.Add(file); // Store .enc files for batch decryption
                }
                else
                {
                    _imageFiles.Add(file); // Add regular image files directly
                }
            }

            if (encryptedFiles.Count > 0)
            {
                DecryptionPassword = await _dialogHandler.ShowPasswordDialogAsync("Enter password: ", xamlRoot);

                if (!string.IsNullOrEmpty(DecryptionPassword))
                {
                    foreach (var encFile in encryptedFiles)
                    {
                        var decryptedFile = await DecryptFileAsync(encFile, DecryptionPassword, xamlRoot);
                        if (decryptedFile != null)
                        {
                            _imageFiles.Add(decryptedFile); // Add decrypted files to the list
                        }
                        else
                        {
                            await _dialogHandler.ShowMessageAsync("Decryption Failed", $"Failed to decrypt {encFile.Name}.", xamlRoot);
                        }
                    }
                }
                else
                {
                    await _dialogHandler.ShowMessageAsync("Decryption Cancelled", "No password entered. .enc files will not be loaded.", xamlRoot);
                }
            }

            return _imageFiles; // All files (decrypted or not) are now ready to be displayed
        }

        public async Task<BitmapImage> LoadImageOrDecryptAsync(StorageFile file, string password)
        {
            // Check if the file is encrypted
            if (file.FileType == ".enc")
            {
                // Attempt to decrypt the file
                var decryptor = new ImageEncryptor(password);
                BitmapImage decryptedImage = decryptor.DecryptImage(file.Path);

                if (decryptedImage != null)
                {
                    return decryptedImage; // Return the decrypted image
                }
                else
                {
                    throw new InvalidOperationException("Decryption failed, invalid password or corrupted file.");
                }
            }
            else
            {
                // If it's not encrypted, just load the image
                return await LoadImageFromFileAsync(file);
            }
        }


        // Encrypt a selected image
        public void EncryptImage(string imagePath, string password)
        {
            var encryptor = new ImageEncryptor(password);
            string encryptedPath = imagePath + ".enc";
            encryptor.EncryptImage(imagePath, encryptedPath);
        }

        // Decrypt a selected encrypted image file
        public async Task<StorageFile> DecryptFileAsync(StorageFile encryptedFile, string password, XamlRoot xamlRoot)
        {
            var encryptor = new ImageEncryptor(password);
            BitmapImage decryptedImage = encryptor.DecryptImage(encryptedFile.Path);

            if (decryptedImage != null)
            {
                // Optionally save the decrypted file somewhere and return it for further processing
                return encryptedFile;
            }

            return null;
        }

        // Picks a folder and loads image references from it
        public async Task<List<StorageFile>> SelectFolderAsync(IntPtr hwnd)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                _imageFiles.Clear();
                var files = await folder.GetFilesAsync();
                bool containsEncrypted = false;
                bool containsUnencrypted = false;

                foreach (var file in files)
                {
                    if (IsSupportedImageFile(file))
                    {
                        containsUnencrypted = true;
                        _imageFiles.Add(file);
                    }
                    else if (file.FileType == ".enc")
                    {
                        containsEncrypted = true;
                    }

                    if (containsEncrypted && containsUnencrypted)
                    {
                        var dialog = new MessageDialog("The folder contains both encrypted and unencrypted files. Please select a folder with only one file type.");
                        await dialog.ShowAsync();
                        _imageFiles.Clear();
                        return null;
                    }
                }
                return _imageFiles;
            }
            return null;
        }

        // Retrieves an image by index, loading it if it's not in cache
        public async Task<BitmapImage> GetImageAsync(int index)
        {
            if (index < 0 || index >= _imageFiles.Count) throw new ArgumentOutOfRangeException(nameof(index));

            // Return cached image if available
            if (_imageCache.TryGetValue(index, out var cachedImage))
                return cachedImage;

            // Load the image on demand
            var file = _imageFiles[index];
            BitmapImage image = await LoadImageFromFileAsync(file);
            _imageCache[index] = image; // Add to cache

            // Manage cache size by removing unused images
            ManageCache(index);

            return image;
        }

        // Checks if the file is an image based on its type
        private bool IsSupportedImageFile(StorageFile file)
        {
            string[] supportedFileTypes = { ".png", ".jpg", ".jpeg" };
            return Array.Exists(supportedFileTypes, ext => ext.Equals(file.FileType, StringComparison.OrdinalIgnoreCase));
        }

        // Loads and resizes an image from a file if it exceeds the maximum dimensions
        public async Task<BitmapImage> LoadImageFromFileAsync(StorageFile file, int maxWidth = 960, int maxHeight = 540)
        {
            using (var stream = await file.OpenReadAsync())
            {
                BitmapImage bitmapImage = new BitmapImage();

                // Set decode pixel width/height only if the image exceeds max dimensions
                bitmapImage.DecodePixelWidth = (int)Math.Min(maxWidth, bitmapImage.PixelWidth);
                bitmapImage.DecodePixelHeight = (int)Math.Min(maxHeight, bitmapImage.PixelHeight);

                await bitmapImage.SetSourceAsync(stream);
                return bitmapImage;
            }
        }

        public List<StorageFile> GetLoadedImages()
        {
            return _imageFiles; // Return the list of loaded images
        }

        // Manages the cache size to ensure it does not exceed the specified CacheSize
        private void ManageCache(int currentIndex)
        {
            // Only keep the current, previous, and next images in cache
            List<int> keysToKeep = new List<int> { currentIndex };

            if (currentIndex > 0)
                keysToKeep.Add(currentIndex - 1);
            if (currentIndex < _imageFiles.Count - 1)
                keysToKeep.Add(currentIndex + 1);

            // Remove all keys that are not in keysToKeep
            foreach (var key in new List<int>(_imageCache.Keys))
            {
                if (!keysToKeep.Contains(key))
                {
                    _imageCache[key] = null;
                    _imageCache.Remove(key);
                }
            }
        }
    }
}
