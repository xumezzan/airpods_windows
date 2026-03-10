using System;
using System.Collections.Concurrent;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using MagicPodsClone.Models;

namespace MagicPodsClone.Services
{
    public class BluetoothWatcherService
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        
        // Cache devices by MAC to handle duplicates and timeouts
        private readonly ConcurrentDictionary<ulong, AirPodsState> _devices = new();

        public event Action<AirPodsState>? DeviceDiscoveredOrUpdated;

        public BluetoothWatcherService()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();

            // Filter for Apple Company ID (0x004C) 
            var manufacturerData = new BluetoothLEManufacturerData
            {
                CompanyId = 0x004C
            };
            
            _watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            _watcher.Received += OnWatcherReceived;
        }

        public void Start()
        {
            if (_watcher.Status != BluetoothLEAdvertisementWatcherStatus.Started)
            {
                _watcher.Start();
            }
        }

        public void Stop()
        {
            if (_watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                _watcher.Stop();
            }
        }

        private void OnWatcherReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var manufacturerSections = args.Advertisement.ManufacturerData;
            
            // Look for Apple payload
            var section = manufacturerSections.FirstOrDefault(x => x.CompanyId == 0x004C);
            if (section != null)
            {
                // Reading Data as a byte array from IBuffer
                byte[] data = new byte[section.Data.Length];
                using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(section.Data))
                {
                    reader.ReadBytes(data);
                }

                // Apple formats their data with a Type indicator. 0x07 is Proximity Pairing (AirPods setup)
                if (data.Length > 0 && data[0] == 0x07)
                {
                    // Basic RSSI filter: we only care about AirPods very close by to avoid picking up 
                    // neighbors. -60 is a decent baseline for "same room / close to PC".
                    if (args.RawSignalStrengthInDBm < -65)
                        return;

                    if (AirPodsPayloadParser.TryParse(data, args.BluetoothAddress, args.RawSignalStrengthInDBm, out var state))
                    {
                        _devices[args.BluetoothAddress] = state;
                        DeviceDiscoveredOrUpdated?.Invoke(state);
                    }
                }
            }
        }
    }
}
