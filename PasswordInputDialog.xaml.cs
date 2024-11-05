using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LockSnap
{
    public sealed partial class PasswordInputDialog : UserControl
    {
        public PasswordInputDialog()
        {
            this.InitializeComponent();
        }

        public string Password => PasswordBox.Password;

        public event RoutedEventHandler OkClicked;
        public event RoutedEventHandler CancelClicked;

        private void OkButton_Click(object sender, RoutedEventArgs e) => OkClicked?.Invoke(this, e);

        private void CancelButton_Click(object sender, RoutedEventArgs e) => CancelClicked?.Invoke(this, e);
    }
}
