// MainWindow.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using WinRT.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Security.Cryptography;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using System.Text;

namespace LockSnap
{
    internal sealed partial class MainWindow : Window
    {
        // Handlers for file handling and encryption
        private FileHandler fileHandler;
        private DialogHandler dialogHandler;
        private List<StorageFile> _loadedImages = new(); // Changed to StorageFile
        private int _currentImageIndex = -1;

        private GalleryHandler galleryHandler;
        private bool _isGalleryMode = false;

        private readonly Dictionary<string, StackPanel> _panelMap;
        private CustomButtonControl _previousButton;

        private readonly Dictionary<CustomButtonControl, string> _buttonTextMap = new();
        private bool _isMenuCollapsed = false;

        public MainWindow()
        {
            this.InitializeComponent();
            dialogHandler = new DialogHandler();
            fileHandler = new FileHandler(this.Content.XamlRoot, dialogHandler);
            galleryHandler = new GalleryHandler(GalleryGridView); 

            // Attach event handlers for UI buttons
            SelectImage.Click += async (s, e) => await LoadImageAsync();
            SelectFolder.Click += async (s, e) => await LoadFolderAsync();
            ModeButton.Click += (s, e) => SwitchMode();
            PreviousButton.Click += (s, e) => ShowPreviousImage();
            NextButton.Click += (s, e) => ShowNextImage();

            // Map each button tag to its corresponding StackPanel for panel toggling
            _panelMap = new Dictionary<string, StackPanel>
            {
                { "Panel0Control", Panel0Control },
                { "Panel1Control", Panel1Control },
                { "Panel2Control", Panel2Control },
                { "Panel3Control", Panel3Control },
                { "Panel4Control", Panel4Control }
            };

            // Initialize button text map for all buttons in the UI
            if (this.Content is DependencyObject content)
            {
                InitializeButtonTextMap(content);
            }

            // Attach event for the hamburger menu button
            HamburgerButton.Click += ToggleMenu;
        }

        // Gets the window handle for use with file pickers
        private IntPtr GetWindowHandle()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            return hwnd;
        }

        // Recursively initializes the text map for all buttons
        private void InitializeButtonTextMap(DependencyObject parent)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is CustomButtonControl customButton)
                {
                    if (!_buttonTextMap.ContainsKey(customButton))
                    {
                        _buttonTextMap[customButton] = customButton.Text; // Save original text
                    }
                }
                else
                {
                    // Recursively search child elements
                    InitializeButtonTextMap(child);
                }
            }
        }

        // Toggles the display of the menu by collapsing button text when in compact view
        private void ToggleMenu(object sender, RoutedEventArgs e)
        {
            _isMenuCollapsed = !_isMenuCollapsed;

            foreach (var button in _buttonTextMap.Keys)
            {
                // Toggle button text visibility
                button.Text = _isMenuCollapsed ? string.Empty : _buttonTextMap[button];

                // Adjust symbol icon margin for compact view
                if (button.FindName("ButtonIcon") is SymbolIcon buttonIcon)
                {
                    buttonIcon.Margin = _isMenuCollapsed ? new Thickness(0) : new Thickness(0, 0, 10, 0);
                }
            }
        }

        // Toggles between showing and hiding panels, with selection indicators
        private void TogglePanel(object sender, RoutedEventArgs e)
        {
            if (sender is CustomButtonControl clickedControl && clickedControl.Tag is string panelName && _panelMap.ContainsKey(panelName))
            {
                var panelToShow = _panelMap[panelName];

                if (panelToShow.Visibility == Visibility.Visible)
                {
                    // Hide the panel and remove selection indicators
                    panelToShow.Visibility = Visibility.Collapsed;
                    clickedControl.IsSelected = false;
                    _previousButton = null;
                }
                else
                {
                    // Hide all panels
                    foreach (var panel in _panelMap.Values)
                    {
                        panel.Visibility = Visibility.Collapsed;
                    }

                    // Show the selected panel
                    panelToShow.Visibility = Visibility.Visible;

                    // Reset the previously selected button, if any
                    if (_previousButton != null)
                    {
                        _previousButton.IsSelected = false;
                    }

                    // Set the clicked control as selected
                    clickedControl.IsSelected = true;
                    _previousButton = clickedControl;
                }
            }
        }

        // Loads images based on selected source (file or folder) and mode
        private async Task LoadAsync(Func<Task<List<StorageFile>>> imageSource)
        {
            var hwnd = GetWindowHandle();
            var imageFiles = await imageSource();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                if (_isGalleryMode)
                {
                    await galleryHandler.LoadImagesAsync(fileHandler.GetLoadedImages());
                }
                else
                {
                    _loadedImages = imageFiles;
                    _currentImageIndex = 0;
                    await DisplayCurrentImageAsync();
                }
            }
        }
        // Loads image/s selected by the user
        private async Task LoadImageAsync()
        {
            await LoadAsync(() => fileHandler.SelectMultipleImagesAsync(GetWindowHandle(), this.Content.XamlRoot));
        }
        // Loads all images from the selected folder
        private async Task LoadFolderAsync()
        {
            await LoadAsync(() => fileHandler.SelectFolderAsync(GetWindowHandle()));
        }
        // Asynchronously displays the image at the current index
        private async Task DisplayCurrentImageAsync()
        {
            if (_currentImageIndex >= 0 && _currentImageIndex < _loadedImages.Count)
            {
                var currentFile = _loadedImages[_currentImageIndex];

                // Access the password from the FileHandler instance
                if (currentFile.FileType == ".enc" && !string.IsNullOrEmpty(fileHandler.DecryptionPassword))
                {
                    try
                    {
                        // Use the stored password for decryption
                        BitmapImage decryptedImage = await fileHandler.LoadImageOrDecryptAsync(currentFile, fileHandler.DecryptionPassword);
                        ImageControl.Source = decryptedImage;
                    }
                    catch (Exception ex)
                    {
                        await dialogHandler.ShowMessageAsync("Decryption Failed", ex.Message, this.Content.XamlRoot);
                    }
                }
                else
                {
                    // Display the non-encrypted image
                    BitmapImage image = await fileHandler.LoadImageFromFileAsync(currentFile);
                    ImageControl.Source = image;
                }
            }
        }

        // Shows the previous image in the loaded image list
        private async void ShowPreviousImage()
        {
            if (_loadedImages.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex - 1 + _loadedImages.Count) % _loadedImages.Count;
                await DisplayCurrentImageAsync();
            }
        }
        // Shows the next image in the loaded image list
        private async void ShowNextImage()
        {
            if (_loadedImages.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex + 1) % _loadedImages.Count;
                await DisplayCurrentImageAsync();
            }
        }

        // Increases opacity of a button when the pointer enters it
        private void OnPointerEnteredButton(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
                button.Opacity = 1;
        }
        // Resets opacity of a button when the pointer exits it
        private void OnPointerExitedButton(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
                button.Opacity = 0;
        }

        // Switches between Preview Mode and Gallery Mode
        private async void SwitchMode()
        {
            _isGalleryMode = !_isGalleryMode;
            ModeButton.Text = _isGalleryMode ? "Gallery Mode" : "Preview Mode";

            if (_isGalleryMode)
            {
                PreviewBorder.Visibility = Visibility.Collapsed;
                GalleryScrollViewer.Visibility = Visibility.Visible;
                await galleryHandler.LoadImagesAsync(fileHandler.GetLoadedImages());
            }
            else
            {
                GalleryScrollViewer.Visibility = Visibility.Collapsed;
                PreviewBorder.Visibility = Visibility.Visible;
                galleryHandler.UnloadImages();
            }
        }

        // Encrypt current image
        private async void EncryptCurrentImage_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedImages.Count > 0 && _currentImageIndex >= 0)
            {
                // Get the currently displayed image
                var currentFile = _loadedImages[_currentImageIndex];

                // Prompt for password
                var password = await dialogHandler.ShowPasswordDialogAsync("Enter password to encrypt", this.Content.XamlRoot);

                if (!string.IsNullOrEmpty(password))
                {
                    // Encrypt the current image
                    fileHandler.EncryptImage(currentFile.Path, password);
                    await dialogHandler.ShowMessageAsync("Encryption Successful", $"File encrypted successfully.", this.Content.XamlRoot);
                }
            }
        }
    }
}
