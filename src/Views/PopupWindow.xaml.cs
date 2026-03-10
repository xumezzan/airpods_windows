using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace AirPodsCompanion.Views
{
    public sealed partial class PopupWindow : Window
    {
        private AppWindow _appWindow;
        public static bool IsOpen { get; private set; } = false;

        public PopupWindow()
        {
            this.InitializeComponent();
            IsOpen = true;
            
            // Remove title bar and make transparent
            ExtendsContentIntoTitleBar = true;
            
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(id);
            
            // Hide standard presenter (titlebar)
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.IsResizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

            // Set size and position (bottom center)
            int width = 350;
            int height = 200;
            
            // Note: In a complete app, we'd calculate monitor size. 
            // For MVP, placing near center-bottom.
            _appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
            
            // Setup auto-close timer
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) => { timer.Stop(); this.Close(); };
            timer.Start();

            this.Closed += (s, e) => { IsOpen = false; };
        }

        public void UpdateData(string name, string battery)
        {
            DeviceNameText.Text = name;
            BatteryStatusText.Text = battery;
        }
    }
}
