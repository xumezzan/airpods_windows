using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace AirPodsCompanion.Services
{
    public class MediaControlService
    {
        private GlobalSystemMediaTransportControlsSessionManager _sessionManager;

        public MediaControlService()
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to init MediaControlService: {ex.Message}");
            }
        }

        public async Task PauseMediaAsync()
        {
            if (_sessionManager == null) return;
            var session = _sessionManager.GetCurrentSession();
            if (session != null)
            {
                await session.TryPauseAsync();
                Debug.WriteLine("Media Paused.");
            }
        }

        public async Task PlayMediaAsync()
        {
            if (_sessionManager == null) return;
            var session = _sessionManager.GetCurrentSession();
            if (session != null)
            {
                await session.TryPlayAsync();
                Debug.WriteLine("Media Played.");
            }
        }
    }
}
