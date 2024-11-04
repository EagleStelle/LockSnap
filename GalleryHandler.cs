// GalleryHandler.cs
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace LockSnap
{
    internal class GalleryHandler
    {
        private readonly GridView _galleryGridView;

        public GalleryHandler(GridView galleryGridView)
        {
            _galleryGridView = galleryGridView;
        }

        // Method to load images into the GridView asynchronously
        public async Task LoadImagesAsync(List<StorageFile> imageFiles)
        {
            UnloadImages();

            foreach (var file in imageFiles)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) // Await the asynchronous call
                {
                    bitmapImage.SetSource(stream);
                }

                _galleryGridView.Items.Add(bitmapImage);
            }
        }

        // Clear images from the gallery
        public void UnloadImages()
        {
            _galleryGridView.Items.Clear();
        }
    }
}
