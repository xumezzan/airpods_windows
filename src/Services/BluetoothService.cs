using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using AirPodsCompanion.Models;

namespace AirPodsCompanion.Services
{
    public class BluetoothService
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        public event EventHandler<AirPodsData> AirPodsUpdated;

        public BluetoothService()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;
            _watcher.Received += OnAdvertisementReceived;
        }

        public void StartScanning()
        {
            try
            {
                _watcher.Start();
                Debug.WriteLine("Started BLE Scanning.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start scanning: {ex.Message}");
            }
        }

        public void StopScanning()
        {
            _watcher.Stop();
            Debug.WriteLine("Stopped BLE Scanning.");
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            foreach (var section in args.Advertisement.ManufacturerData)
            {
                // 0x004C is Apple's Company ID
                if (section.CompanyId == 0x004C) 
                {
                    var data = new byte[section.Data.Length];
                    using (var reader = DataReader.FromBuffer(section.Data))
                    {
                        reader.ReadBytes(data);
                    }

                    // Apple's beacon format for AirPods starts with 0x07 and length >= 25 (0x19)
                    if (data.Length >= 14 && data[0] == 0x07 && data[1] >= 0x19)
                    {
                        AirPodsModel model = IdentifyModel(data[3]);

                        // Decode battery
                        int rightBat = (data[12] & 0x0F) == 0x0F ? 15 : (data[12] & 0x0F) * 10;
                        int leftBat = ((data[12] & 0xF0) >> 4) == 0x0F ? 15 : ((data[12] & 0xF0) >> 4) * 10;
                        int caseBat = (data[13] & 0x0F) == 0x0F ? 15 : (data[13] & 0x0F) * 10;

                        // Max handling
                        if (model == AirPodsModel.AirPodsMax)
                        {
                            leftBat = Math.Max(leftBat == 15 ? -1 : leftBat, rightBat == 15 ? -1 : rightBat);
                            if (leftBat == -1) leftBat = 15;
                            rightBat = 15;
                            caseBat = 15;
                        }

                        // Determine in-ear state
                        bool inEar = ((data[11] & 0x08) != 0) || ((data[11] & 0x02) != 0);
                        bool isCharging = (data[13] & 0x10) != 0;

                        var airpodsData = new AirPodsData
                        {
                            MacAddress = args.BluetoothAddress,
                            Model = model,
                            LeftBattery = leftBat,
                            RightBattery = rightBat,
                            CaseBattery = caseBat,
                            InEar = inEar,
                            IsCharging = isCharging,
                            LastSeen = DateTime.Now
                        };

                        AirPodsUpdated?.Invoke(this, airpodsData);
                    }
                }
            }
        }

        private AirPodsModel IdentifyModel(byte modelId)
        {
            return modelId switch
            {
                0x02 => AirPodsModel.AirPods1,
                0x0F => AirPodsModel.AirPods2,
                0x13 => AirPodsModel.AirPods3,
                0x22 => AirPodsModel.AirPods4,
                0x0E => AirPodsModel.AirPodsPro1,
                0x14 => AirPodsModel.AirPodsPro2,
                0x0A => AirPodsModel.AirPodsMax,
                0x0B => AirPodsModel.PowerbeatsPro,
                0x12 => AirPodsModel.BeatsFitPro,
                0x11 => AirPodsModel.BeatsStudioBuds,
                _ => AirPodsModel.Unknown,
            };
        }
    }
}
