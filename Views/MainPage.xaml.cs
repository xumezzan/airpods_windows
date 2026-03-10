using System;
using AirPodsCompanion.Services;
using AirPodsCompanion.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace AirPodsCompanion.Views
{
    public sealed partial class MainPage : Page
    {
        private BluetoothService _btService;
        private GattService _gattService;
        private MediaControlService _mediaService;
        
        private ulong _lastMacAddress = 0;
        private bool _lastInEar = false;

        public MainPage()
        {
            this.InitializeComponent();
            _btService = new BluetoothService();
            _gattService = new GattService();
            _mediaService = new MediaControlService();
            
            _btService.AirPodsUpdated += OnAirPodsUpdated;
            _btService.StartScanning();
        }

        private void OnAirPodsUpdated(object sender, AirPodsData data)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                _lastMacAddress = data.MacAddress;
                
                string name = data.Model.ToString().Replace("AirPodsPro", "AirPods Pro ");
                DeviceNameText.Text = $"{name} Connected";
                BatteryStatusText.Text = data.GetBatteryString();

                // Advanced Auto-pause logic
                if (AutoPauseToggle.IsOn)
                {
                    if (_lastInEar && !data.InEar) 
                        await _mediaService.PauseMediaAsync();
                    else if (!_lastInEar && data.InEar)
                        await _mediaService.PlayMediaAsync();
                }

                _lastInEar = data.InEar;

                // Fire Popup if enabled
                if (PopupToggle.IsOn && !PopupWindow.IsOpen && data.RSSI > -60 && data.LidOpen)
                {
                    var popup = new PopupWindow();
                    popup.UpdateData(name, data.GetBatteryString());
                    popup.Activate();
                }
            });
        }

        private async void AncBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_lastMacAddress != 0)
                await _gattService.SetNoiseControlModeAsync(_lastMacAddress, NoiseControlMode.NoiseCancellation);
        }

        private async void TransBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_lastMacAddress != 0)
                await _gattService.SetNoiseControlModeAsync(_lastMacAddress, NoiseControlMode.Transparency);
        }

        private async void OffBtn_Click(object sender, RoutedEventArgs e)
        {
             if (_lastMacAddress != 0)
                await _gattService.SetNoiseControlModeAsync(_lastMacAddress, NoiseControlMode.Off);
        }

        private void AutoPauseToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Optional: Save preference
        }
    }
}
