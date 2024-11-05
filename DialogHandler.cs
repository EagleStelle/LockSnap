// DialogHandler.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

public class DialogHandler
{
    public async Task<string> ShowPasswordDialogAsync(string title, XamlRoot xamlRoot)
    {
        var inputDialog = new ContentDialog
        {
            Title = title,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = xamlRoot
        };

        var passwordBox = new TextBox
        {
            PlaceholderText = "Enter password",
            AcceptsReturn = false,
            MaxLength = 50
        };
        inputDialog.Content = passwordBox;

        var result = await inputDialog.ShowAsync();
        return result == ContentDialogResult.Primary ? passwordBox.Text : string.Empty;
    }

    public async Task ShowMessageAsync(string title, string message, XamlRoot xamlRoot)
    {
        var messageDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = xamlRoot
        };

        await messageDialog.ShowAsync();
    }
}
