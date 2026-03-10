using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace AirPodsCompanion.Services
{
    // Minimalistic tray service using PInvoke since WinUI 3 doesn't have native NotifyIcon yet
    public class TrayService : IDisposable
    {
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA pnid);

        [DllImport("user32.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        private NOTIFYICONDATA _nid;
        private bool _isAdded;

        public TrayService(IntPtr hwnd)
        {
            _nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = hwnd,
                uID = 1,
                uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage = 0x0400 + 1, // WM_USER + 1
                hIcon = LoadIcon(IntPtr.Zero, (IntPtr)32512), // IDI_APPLICATION
                szTip = "AirPods Companion"
            };
        }

        public void Show()
        {
            if (!_isAdded)
            {
                Shell_NotifyIcon(NIM_ADD, ref _nid);
                _isAdded = true;
            }
        }

        public void UpdateTooltip(string text)
        {
            if (_isAdded)
            {
                _nid.szTip = text.Length > 127 ? text.Substring(0, 127) : text;
                Shell_NotifyIcon(NIM_MODIFY, ref _nid);
            }
        }

        public void Hide()
        {
            if (_isAdded)
            {
                Shell_NotifyIcon(NIM_DELETE, ref _nid);
                _isAdded = false;
            }
        }

        public void Dispose()
        {
            Hide();
        }
    }
}
