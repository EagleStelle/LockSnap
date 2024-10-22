// MainWindow.xaml.cs
using Microsoft.UI.Xaml;
using System.Linq;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Input;

namespace LockSnap
{
    public sealed partial class MainWindow : Window
    {
        private string selectedImagePath;
        private string selectedFolderPath;

        // Dictionary to store decrypted images
        private Dictionary<string, BitmapImage> decryptedImages = new Dictionary<string, BitmapImage>();
        private int currentImageIndex = 0;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        // Open file picker to select an image to encrypt
        private async Task SelectImageAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            // File picker works with desktop apps through COM interop
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                selectedImagePath = file.Path;
                DisplayMessage("Image selected: " + file.Name, true);
            }
        }
        private async void SelectImageButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            await SelectImageAsync();
        }
        private async void SelectImageFlyout_Click(object sender, RoutedEventArgs e)
        {
            await SelectImageAsync();
        }
        private async void SelectDirectoryFlyout_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            // Folder picker works with desktop apps through COM interop
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                selectedFolderPath = folder.Path;
                DisplayMessage("Folder selected for encryption: " + folder.Name, true);
            }
        }

        private async Task SelectArtifactAsync()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".enc");

            // File picker works with desktop apps through COM interop
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                selectedImagePath = file.Path;
                DisplayMessage("Encrypted file selected: " + file.Name, false);
            }
        }
        private async void SelectArtifactButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            await SelectArtifactAsync();
        }
        private async void SelectArtifactFlyout_Click(object sender, RoutedEventArgs e)
        {
            await SelectArtifactAsync();
        }
        // Decrypt folder button click event to set the folder path
        private async void SelectArchiveFlyout_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Folder picker works with desktop apps through COM interop
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                selectedFolderPath = folder.Path;
                DisplayMessage("Folder selected for decryption: " + folder.Name, false);
            }
        }

        // Encrypt button click event
        private void HandleEncryption_Click(object sender, RoutedEventArgs e)
        {
            // Check if folder is selected
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                // Directory mode: Encrypt all images in the selected folder
                try
                {
                    var imageFiles = Directory.GetFiles(selectedFolderPath, "*.*")
                                            .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"));

                    string password = PasswordBoxEncrypt.Password;
                    ImageEncryptor encryptor = new ImageEncryptor(password);

                    foreach (var imagePath in imageFiles)
                    {
                        string encryptedPath = imagePath + ".enc";
                        encryptor.EncryptImage(imagePath, encryptedPath);
                    }

                    DisplayMessage("All images in the folder encrypted successfully!", true);
                }
                catch (Exception ex)
                {
                    DisplayMessage("Folder encryption failed: " + ex.Message, true);
                }
            }
            else if (!string.IsNullOrEmpty(selectedImagePath))  // Single image file mode
            {
                // Single file mode: encrypt the selected image
                try
                {
                    string encryptedPath = selectedImagePath + ".enc";
                    string password = PasswordBoxEncrypt.Password;

                    ImageEncryptor encryptor = new ImageEncryptor(password);
                    encryptor.EncryptImage(selectedImagePath, encryptedPath);

                    DisplayMessage("Image encrypted successfully!", true);
                }
                catch (Exception ex)
                {
                    DisplayMessage("Encryption failed: " + ex.Message, true);
                }
            }
            else
            {
                DisplayMessage("Please select an image or a folder first.", true);
            }
        }

        // Decrypt button click event
        private void HandleDecryption_Click(object sender, RoutedEventArgs e)
        {
            decryptedImages.Clear(); // Clear the dictionary before storing new images

            string password = PasswordBoxDecrypt.Password;
            ImageEncryptor encryptor = new ImageEncryptor(password);

            // Directory mode: Decrypt all .enc files in the selected folder
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                try
                {
                    var encryptedFiles = Directory.GetFiles(selectedFolderPath, "*.enc");

                    foreach (var encryptedPath in encryptedFiles)
                    {
                        BitmapImage decryptedImage = encryptor.DecryptImage(encryptedPath);
                        string fileName = Path.GetFileName(encryptedPath);

                        decryptedImages[fileName] = decryptedImage;
                    }

                    DisplayMessage("All images in the folder decrypted successfully!", false);

                    if (decryptedImages.Count > 0)
                    {
                        currentImageIndex = 0; // Start at the first image
                        DisplayDecryptedImage(currentImageIndex); // Display the first image
                    }
                }
                catch (Exception ex)
                {
                    DisplayMessage("Folder decryption failed: " + ex.Message, false);
                }
            }
            else if (!string.IsNullOrEmpty(selectedImagePath))  // Single file mode
            {
                try
                {
                    BitmapImage decryptedImage = encryptor.DecryptImage(selectedImagePath);
                    string fileName = Path.GetFileName(selectedImagePath);

                    decryptedImages[fileName] = decryptedImage;

                    DisplayMessage("Image decrypted successfully!", false);

                    currentImageIndex = 0; // Since only one image is decrypted
                    DisplayDecryptedImage(currentImageIndex);
                }
                catch (Exception ex)
                {
                    DisplayMessage("Decryption failed: " + ex.Message, false);
                }
            }
            else
            {
                DisplayMessage("Please select an encrypted file or a folder first.", false);
            }
        }

        // Helper method to displays for encryption or decryption
        private void DisplayDecryptedImage(int index)
        {
            if (decryptedImages.Count > 0)
            {
                // Get the file name at the current index
                string fileName = decryptedImages.Keys.ElementAt(index);

                // Get the corresponding BitmapImage
                BitmapImage decryptedImage = decryptedImages[fileName];

                // Display the image in the Image control
                DecryptedImageControl.Source = decryptedImage;

                // Optionally display the image file name in the UI
                DisplayMessage($"Displaying: {fileName}", false);
            }
        }
        // Go to the previous image
        private void PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            if (decryptedImages.Count > 0)
            {
                currentImageIndex--;
                if (currentImageIndex < 0) currentImageIndex = decryptedImages.Count - 1; // Wrap around to the last image
                DisplayDecryptedImage(currentImageIndex);
            }
        }
        // Go to the next image
        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (decryptedImages.Count > 0)
            {
                currentImageIndex++;
                if (currentImageIndex >= decryptedImages.Count) currentImageIndex = 0; // Wrap around to the first image
                DisplayDecryptedImage(currentImageIndex);
            }
        }
        // Event handlers for PointerEntered and PointerExited to control button visibility
        private void OnPointerEnteredButton(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Opacity = 1; // Show button on hover
        }

        private void OnPointerExitedButton(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Opacity = 0; // Hide button when not hovering
        }

        private void DisplayMessage(string message, bool isEncrypt)
        {
            MessageTextBlock.Text = message;
        }
        private async Task LoadDecryptedImageAsync(string imagePath)
        {
            var file = await StorageFile.GetFileFromPathAsync(imagePath);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);

            DecryptedImageControl.Source = bitmapImage;  // Assuming you have an Image control named DecryptedImageControl
        }
    }
}
