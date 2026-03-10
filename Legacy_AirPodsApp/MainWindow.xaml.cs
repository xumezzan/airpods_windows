using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AirPodsApp
{
    public class DeviceItem : System.ComponentModel.INotifyPropertyChanged
    {
        private ulong _macAddress;
        private string _emojiIcon;
        private string _name;
        private string _status;
        private bool _isConnected;
        private DateTime _lastSeen;
        private int _leftBattery = 15;
        private int _rightBattery = 15;
        private int _caseBattery = 15;

        public ulong MacAddress { get => _macAddress; set { _macAddress = value; Notify("MacAddress"); } }
        public string EmojiIcon { get => _emojiIcon; set { _emojiIcon = value; Notify("EmojiIcon"); } }
        public string Name { get => _name; set { _name = value; Notify("Name"); } }
        public string Status { get => _status; set { _status = value; Notify("Status"); } }
        public DateTime LastSeen { get => _lastSeen; set { _lastSeen = value; Notify("LastSeen"); } }
        public bool IsConnected { get => _isConnected; set { _isConnected = value; Notify("IsConnected"); Notify("ButtonText"); } }

        public void UpdateBattery(int left, int right, int c)
        {
            if (left != 15) _leftBattery = left;
            if (right != 15) _rightBattery = right;
            if (c != 15) _caseBattery = c;
            
            string lStr = _leftBattery == 15 ? "--" : $"{_leftBattery}%";
            string rStr = _rightBattery == 15 ? "--" : $"{_rightBattery}%";
            string cStr = _caseBattery == 15 ? "--" : $"{_caseBattery}%";
            
            Status = $"Левый: {lStr}  Правый: {rStr}  Кейс: {cStr}";
        }

        public string ButtonText => IsConnected ? "Отключить" : "Подключить";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void Notify(string prop) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
    }

    public partial class MainWindow : Window
    {
        private AirPodsService _service;
        public ObservableCollection<DeviceItem> Devices { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            Devices = new ObservableCollection<DeviceItem>();
            DeviceList.ItemsSource = Devices;

            _service = new AirPodsService();
            _service.AirPodsUpdated += Service_AirPodsUpdated;
            _service.Start();

            DispatcherTimer cleanupTimer = new DispatcherTimer();
            cleanupTimer.Interval = TimeSpan.FromSeconds(5);
            cleanupTimer.Tick += CleanupTimer_Tick;
            cleanupTimer.Start();
        }

        private void CleanupTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                if ((now - Devices[i].LastSeen).TotalSeconds > 15) Devices.RemoveAt(i);
            }
            EmptyStateText.Visibility = Devices.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Service_AirPodsUpdated(object sender, AirPodsData data)
        {
            Dispatcher.Invoke(() =>
            {
                DeviceItem realDevice = Devices.Count > 0 ? Devices[0] : null;

                if (realDevice == null)
                {
                    realDevice = new DeviceItem { MacAddress = data.MacAddress, IsConnected = true };
                    Devices.Add(realDevice);
                }
                
                realDevice.LastSeen = DateTime.Now;
                realDevice.Name = data.Model.ToString().Replace("AirPodsPro", "AirPods Pro ");
                realDevice.EmojiIcon = "🎧";
                realDevice.UpdateBattery(data.LeftBattery, data.RightBattery, data.CaseBattery);
                realDevice.IsConnected = true;

                EmptyStateText.Visibility = Visibility.Collapsed;
            });
        }

        private void SidebarList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarList == null || HeadphonesPage == null || SettingsPage == null || 
                HotkeysPage == null || PreferencesPage == null || SupportPage == null) return;

            if (SidebarList.SelectedItem is ListBoxItem item)
            {
                string tag = item.Tag?.ToString();
                
                // Hide all pages
                HeadphonesPage.Visibility = Visibility.Collapsed;
                SettingsPage.Visibility = Visibility.Collapsed;
                HotkeysPage.Visibility = Visibility.Collapsed;
                PreferencesPage.Visibility = Visibility.Collapsed;
                SupportPage.Visibility = Visibility.Collapsed;
                
                // Show selected page
                if (tag == "Headphones") HeadphonesPage.Visibility = Visibility.Visible;
                else if (tag == "Settings") SettingsPage.Visibility = Visibility.Visible;
                else if (tag == "Hotkeys") HotkeysPage.Visibility = Visibility.Visible;
                else if (tag == "Preferences") PreferencesPage.Visibility = Visibility.Visible;
                else if (tag == "Support") SupportPage.Visibility = Visibility.Visible;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Button_Click(object sender, RoutedEventArgs e) { /* Placeholder */ }

        protected override void OnClosed(EventArgs e)
        {
            _service.Stop();
            base.OnClosed(e);
        }
    }
}
