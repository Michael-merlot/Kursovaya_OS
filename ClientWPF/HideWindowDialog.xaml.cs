using System.Windows;

namespace ClientWPF
{
    public partial class HideWindowDialog : Window
    {
        public int SelectedTime { get; private set; } = 5000;

        public HideWindowDialog()
        {
            InitializeComponent();
            timeSlider.Value = SelectedTime;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedTime = (int)timeSlider.Value;
            DialogResult = true;
        }
    }
}