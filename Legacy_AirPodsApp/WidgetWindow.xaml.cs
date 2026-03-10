using System.Windows;
using System.Windows.Input;

namespace AirPodsApp
{
    public partial class WidgetWindow : Window
    {
        public WidgetWindow()
        {
            InitializeComponent();
            
            // Start at bottom right of the primary screen, slightly above taskbar
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 20;
            this.Top = desktopWorkingArea.Bottom - this.Height - 20;
        }

        public void UpdateData(AirPodsData data)
        {
            Dispatcher.Invoke(() =>
            {
                LeftBatText.Text = data.LeftBattery == 15 ? "L: --" : $"L: {data.LeftBattery}%";
                RightBatText.Text = data.RightBattery == 15 ? "R: --" : $"R: {data.RightBattery}%";
                CaseBatText.Text = data.CaseBattery == 15 ? "C: --" : $"C: {data.CaseBattery}%";
            });
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
