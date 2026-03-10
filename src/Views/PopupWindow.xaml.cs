using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;

namespace AirPodsCompanion.Views
{
    public sealed partial class PopupWindow : Window
    {
        private AppWindow _appWindow;
        public static bool IsOpen { get; private set; } = false;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        public PopupWindow()
        {
            this.InitializeComponent();
            IsOpen = true;
            
            if (this.Content is FrameworkElement rootBlock)
            {
                rootBlock.Loaded += RootGrid_Loaded;
            }
            
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
            
            // Set TopMost
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            
            // Setup auto-close timer
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) => { timer.Stop(); this.Close(); };
            timer.Start();

            this.Closed += (s, e) => { IsOpen = false; };
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Content is Grid rootGrid)
            {
                var mainBorder = rootGrid.Children.Count > 0 ? rootGrid.Children[0] as Border : null;
                if (mainBorder != null)
                {
                    mainBorder.Opacity = 0;
                    var translate = new Microsoft.UI.Xaml.Media.TranslateTransform { Y = 50 };
                    mainBorder.RenderTransform = translate;

                    var sb = new Storyboard();
                    
                    var opacityAnim = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(400)
                    };
                    Storyboard.SetTarget(opacityAnim, mainBorder);
                    Storyboard.SetTargetProperty(opacityAnim, "Opacity");
                    
                    var slideAnim = new DoubleAnimation
                    {
                        From = 50,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(400),
                        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
                    };
                    Storyboard.SetTarget(slideAnim, translate);
                    Storyboard.SetTargetProperty(slideAnim, "Y");

                    sb.Children.Add(opacityAnim);
                    sb.Children.Add(slideAnim);
                    
                    sb.Begin();
                }
            }
        }

        public void UpdateData(string name, string battery)
        {
            DeviceNameText.Text = name;
            BatteryStatusText.Text = battery;
        }
    }
}
