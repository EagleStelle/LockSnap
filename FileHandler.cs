// FileHandler.cs
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LockSnap
{
    internal class FileHandler
    {
        // Picks a single image from file picker
        public async Task<string> SelectSingleImageAsync(IntPtr hwnd)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file?.Path;
        }

        // Picks a folder from folder picker
        public async Task<string> SelectFolderAsync(IntPtr hwnd)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder?.Path;
        }

        // Picks an encrypted file from file picker
        public async Task<string> SelectEncryptedFileAsync(IntPtr hwnd)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".enc");

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file?.Path;
        }
    }
}
