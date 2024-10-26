// Main.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;

namespace LockSnap
{
    internal sealed partial class MainWindow : Window
    {
        private string selectedImagePath;
        private string selectedFolderPath;
        private Dictionary<string, BitmapImage> decryptedImages = new Dictionary<string, BitmapImage>();
        private int currentImageIndex = 0;

        // Handlers for file handling and encryption
        private FileHandler fileHandler;
        private EncryptionHandler encryptionHandler;
        private UIHandler uiHandler;

        public MainWindow()
        {
            this.InitializeComponent();
            fileHandler = new FileHandler();
            uiHandler = new UIHandler(MessageTextBlock, DecryptedImageControl, EncryptButton, DecryptButton);
        }

        // Event Handlers

        // Button: Select Image (for SplitButton)
        private async void SelectImageButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedImagePath = await fileHandler.SelectSingleImageAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                uiHandler.DisplayMessage("Image selected: " + selectedImagePath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: true);
            }
        }

        // Button: Select Image (Flyout)
        private async void SelectImageFlyout_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedImagePath = await fileHandler.SelectSingleImageAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                uiHandler.DisplayMessage("Image selected: " + selectedImagePath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: true);
            }
        }

        // Button: Select Directory (Flyout)
        private async void SelectDirectoryFlyout_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedFolderPath = await fileHandler.SelectFolderAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                uiHandler.DisplayMessage("Folder selected: " + selectedFolderPath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: true);
            }
        }

        // Button: Select Encrypted File (for SplitButton)
        private async void SelectArtifactButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedImagePath = await fileHandler.SelectEncryptedFileAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                uiHandler.DisplayMessage("Encrypted file selected: " + selectedImagePath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: false);
            }
        }

        // Button: Select Encrypted File (Flyout)
        private async void SelectArtifactFlyout_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedImagePath = await fileHandler.SelectEncryptedFileAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                uiHandler.DisplayMessage("Encrypted file selected: " + selectedImagePath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: false);
            }
        }

        // Button: Select Archive (Flyout)
        private async void SelectArchiveFlyout_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            selectedFolderPath = await fileHandler.SelectFolderAsync(hwnd);
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                uiHandler.DisplayMessage("Archive folder selected: " + selectedFolderPath);
                uiHandler.UpdateButtonStates(selectedImagePath, selectedFolderPath, isEncryption: false);
            }
        }

        // Button: Navigate to Previous Image
        private void PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            uiHandler.ShowPreviousImage(); // Delegate the navigation to UIHandler
        }

        // Button: Navigate to Next Image
        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            uiHandler.ShowNextImage(); // Delegate the navigation to UIHandler
        }

        // Button: OnPointerEntered
        private void OnPointerEnteredButton(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Opacity = 1; // Show button on hover
        }

        // Button: OnPointerExited
        private void OnPointerExitedButton(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Opacity = 0; // Hide button when not hovering
        }


        // Button: Encrypt Images
        private void OnEncryptButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBoxEncrypt.Password;
            encryptionHandler = new EncryptionHandler(password);

            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                encryptionHandler.EncryptImagesInFolder(selectedFolderPath);
                uiHandler.DisplayMessage("Images in folder encrypted successfully!");
            }
            else if (!string.IsNullOrEmpty(selectedImagePath))
            {
                encryptionHandler.EncryptSingleImage(selectedImagePath);
                uiHandler.DisplayMessage("Image encrypted successfully!");
            }
        }

        // Button: Decrypt Images
        private void OnDecryptButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBoxDecrypt.Password;
            encryptionHandler = new EncryptionHandler(password);

            try
            {
                if (!string.IsNullOrEmpty(selectedFolderPath))
                {
                    decryptedImages = encryptionHandler.DecryptImagesInFolder(selectedFolderPath);
                    uiHandler.SetDecryptedImages(decryptedImages);
                    uiHandler.DisplayMessage("Images in folder decrypted successfully!");
                    uiHandler.DisplayCurrentDecryptedImage();
                }
                else if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    BitmapImage decryptedImage = encryptionHandler.DecryptSingleImage(selectedImagePath);
                    uiHandler.DisplayMessage("Image decrypted successfully!");
                    DecryptedImageControl.Source = decryptedImage;
                }
            }
            catch (Exception ex)
            {
                // Display an error message to the user
                uiHandler.DisplayMessage("Decryption failed: Invalid password or corrupted file.");
                // Optionally log the exception for debugging purposes
                Console.WriteLine(ex.Message);
            }
        }
    }
}
