using Avalonia.Controls;

namespace srra
{
    public partial class MessageBox : Window
    {
        public MessageBox(string message, string buttonMessage) :base()
        {
            InitializeComponent();
            var messageBoxViewModel = new MessageBoxViewModel() {
                Message = message,
            };
            MessageBoxOKButton.Content = buttonMessage;
            DataContext = messageBoxViewModel;
            MessageBoxOKButton.Click += MessageBoxOKButton_Click;
        }

        public MessageBox()
        {
            InitializeComponent();
        }

        private void MessageBoxOKButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
    }

    public class MessageBoxViewModel
    {
        public string? Message { get; set; }
    }
}