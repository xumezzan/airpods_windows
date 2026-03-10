using System;
using System.Runtime.InteropServices;

namespace AirPodsApp
{
    public static class MediaController
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static bool _wasInEar = true;

        public static void HandleInEarDetection(bool currentlyInEar)
        {
            if (_wasInEar && !currentlyInEar)
            {
                // Removed from ear (Pause)
                ToggleMedia();
                _wasInEar = false;
            }
            else if (!_wasInEar && currentlyInEar)
            {
                // Put back in ear (Play)
                ToggleMedia();
                _wasInEar = true;
            }
        }

        private static void ToggleMedia()
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
