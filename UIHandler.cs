// UIHandler.cs
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;

namespace LockSnap
{
    internal class UIHandler
    {
        private TextBlock messageTextBlock;
        private Image decryptedImageControl;
        private Dictionary<string, BitmapImage> decryptedImages;
        private int currentImageIndex;

        public UIHandler(TextBlock messageTextBlock, Image decryptedImageControl)
        {
            this.messageTextBlock = messageTextBlock;
            this.decryptedImageControl = decryptedImageControl;
            this.decryptedImages = new Dictionary<string, BitmapImage>();
            this.currentImageIndex = 0;
        }

        public void SetDecryptedImages(Dictionary<string, BitmapImage> images, int startingIndex = 0)
        {
            decryptedImages = images;
            currentImageIndex = startingIndex;
        }

        public void DisplayMessage(string message)
        {
            messageTextBlock.Text = message;
        }

        public void DisplayCurrentDecryptedImage()
        {
            if (decryptedImages != null && decryptedImages.Count > 0)
            {
                var imageKey = new List<string>(decryptedImages.Keys)[currentImageIndex];
                decryptedImageControl.Source = decryptedImages[imageKey];
                DisplayMessage(imageKey);
            }
            else
            {
                DisplayMessage("No images to display.");
            }
        }

        public void ShowNextImage()
        {
            if (decryptedImages != null && currentImageIndex < decryptedImages.Count - 1)
            {
                currentImageIndex++;
                DisplayCurrentDecryptedImage();
            }
        }

        public void ShowPreviousImage()
        {
            if (decryptedImages != null && currentImageIndex > 0)
            {
                currentImageIndex--;
                DisplayCurrentDecryptedImage();
            }
        }
    }
}
