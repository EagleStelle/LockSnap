using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using Microsoft.UI;
using Microsoft.UI.Text;

namespace LockSnap
{
    public sealed partial class CustomButtonControl : UserControl
    {
        public CustomButtonControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty IconSymbolProperty =
            DependencyProperty.Register(nameof(IconSymbol), typeof(Symbol), typeof(CustomButtonControl), new PropertyMetadata(Symbol.Emoji));

        public Symbol IconSymbol
        {
            get => (Symbol)GetValue(IconSymbolProperty);
            set => SetValue(IconSymbolProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(CustomButtonControl), new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CustomButtonControl), new PropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsIndicationEnabledProperty =
            DependencyProperty.Register(nameof(IsIndicationEnabled), typeof(bool), typeof(CustomButtonControl), new PropertyMetadata(true));

        public bool IsIndicationEnabled
        {
            get => (bool)GetValue(IsIndicationEnabledProperty);
            set => SetValue(IsIndicationEnabledProperty, value);
        }

        public static readonly DependencyProperty SelectedBackgroundColorProperty =
            DependencyProperty.Register(nameof(SelectedBackgroundColor), typeof(Brush), typeof(CustomButtonControl), new PropertyMetadata(new SolidColorBrush(Colors.DarkSlateGray)));

        public Brush SelectedBackgroundColor
        {
            get => (Brush)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        public static readonly DependencyProperty VerticalBarColorProperty =
            DependencyProperty.Register(nameof(VerticalBarColor), typeof(Brush), typeof(CustomButtonControl), new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        public Brush VerticalBarColor
        {
            get => (Brush)GetValue(VerticalBarColorProperty);
            set => SetValue(VerticalBarColorProperty, value);
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomButtonControl control && e.NewValue is bool isSelected)
            {
                if (control.IsIndicationEnabled && isSelected)
                {
                    // Apply selected styles
                    control.MainButton.Background = control.SelectedBackgroundColor;
                    control.ButtonText.FontWeight = FontWeights.Bold;
                    control.MainButton.BorderBrush = control.VerticalBarColor;
                    control.MainButton.BorderThickness = new Thickness(0, 0, 5, 0);
                }
                else
                {
                    // Reset styles
                    control.MainButton.Background = new SolidColorBrush(Colors.Transparent);
                    control.ButtonText.FontWeight = FontWeights.Normal;
                    control.MainButton.BorderBrush = null;
                    control.MainButton.BorderThickness = new Thickness(0);
                }
            }
        }

        public event RoutedEventHandler Click;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
