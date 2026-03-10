using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MagicPodsClone.Services;
using MagicPodsClone.Models;

namespace MagicPodsClone;

public partial class MainWindow : Window
{
    private readonly BluetoothWatcherService _watcher;
    private DispatcherTimer _hideTimer;
    private bool _isVisible = false;
    private ulong _trackedMacAddress = 0;

    public MainWindow()
    {
        InitializeComponent();

        // Position window at bottom center, above the taskbar
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;
        double windowWidth = 300;
        double windowHeight = 150;
        
        Left = (screenWidth - windowWidth) / 2;
        Top = screenHeight - windowHeight - 50; // 50px offset from bottom

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _hideTimer.Tick += HideTimer_Tick;

        _watcher = new BluetoothWatcherService();
        _watcher.DeviceDiscoveredOrUpdated += OnDeviceDiscovered;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _watcher.Start();
    }

    private void OnDeviceDiscovered(AirPodsState state)
    {
        Dispatcher.BeginInvoke(() =>
        {
            // If we are tracking a different device, ignore for now (simplistic logic)
            if (_trackedMacAddress != 0 && _trackedMacAddress != state.MacAddress) 
                return;

            _trackedMacAddress = state.MacAddress;

            if (state.IsLidOpen)
            {
                UpdateUI(state);

                if (!_isVisible)
                {
                    _isVisible = true;
                    // Play Entrance Animation
                    if (Resources["EntranceAnimation"] is Storyboard sb)
                    {
                        sb.Begin();
                    }
                }
                
                // Reset hide timer
                _hideTimer.Stop();
                _hideTimer.Start();
            }
        });
    }

    private void UpdateUI(AirPodsState state)
    {
        LeftBatText.Text = state.LeftBattery >= 0 ? $"{state.LeftBattery}%" : "--%";
        RightBatText.Text = state.RightBattery >= 0 ? $"{state.RightBattery}%" : "--%";
        CaseBatText.Text = state.CaseBattery >= 0 ? $"{state.CaseBattery}%" : "--%";
    }

    private void HideTimer_Tick(object? sender, EventArgs e)
    {
        _hideTimer.Stop();
        _isVisible = false;
        
        // Simple fade out
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
        fadeOut.Completed += (s, args) => MainBorder.Opacity = 0;
        MainBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);

        // Allow to track a different device next time if it vanished
        _trackedMacAddress = 0;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _watcher.Stop();
        base.OnClosing(e);
    }
}