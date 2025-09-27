using System.Windows;

namespace Genisis.Presentation.Wpf.Views
{
    public partial class InputDialog : Window
    {
        public string ResponseText => ResponseTextBox.Text;

        public InputDialog(string prompt, string title = "Input", string defaultValue = "")
        {
            InitializeComponent();
            this.Title = title;
            PromptText.Text = prompt;
            ResponseTextBox.Text = defaultValue;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ResponseTextBox.Text))
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("The field cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResponseTextBox.Focus();
            ResponseTextBox.SelectAll();
        }
    }
}
