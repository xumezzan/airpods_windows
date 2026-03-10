using System;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace AirPodsApp
{
    public enum AirPodsModel
    {
        Unknown,
        AirPods1,
        AirPods2,
        AirPods3,
        AirPods4,
        AirPodsPro1,
        AirPodsPro2,
        AirPodsMax,
        PowerbeatsPro,
        BeatsFitPro,
        BeatsStudioBuds
    }

    public class AirPodsData
    {
        public ulong MacAddress { get; set; }
        public AirPodsModel Model { get; set; } = AirPodsModel.Unknown;
        public int LeftBattery { get; set; } = 15;
        public int RightBattery { get; set; } = 15;
        public int CaseBattery { get; set; } = 15;
        public bool InEar { get; set; } = true;
    }

    public class AirPodsService
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        public event EventHandler<AirPodsData> AirPodsUpdated;
        private DateTime _lastUpdate = DateTime.MinValue;

        public AirPodsService()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;
            _watcher.Received += OnAdvertisementReceived;
        }

        public void Start() => _watcher.Start();
        public void Stop() => _watcher.Stop();

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            foreach (var section in args.Advertisement.ManufacturerData)
            {
                if (section.CompanyId == 0x004C) // Apple
                {
                    var data = new byte[section.Data.Length];
                    using (var reader = DataReader.FromBuffer(section.Data))
                    {
                        reader.ReadBytes(data);
                    }

                    // Payload structure parsing
                    if (data.Length >= 14 && data[0] == 0x07 && data[1] >= 0x19) 
                    {
                        // data[2] and data[3] often indicate the device type/state, but data[3] alone is a good proxy for model.
                        // However, commonly for Apple 0x07 beacon, the device model ID is encoded in byte [3] or byte [2].
                        byte modelId = data[3];
                        AirPodsModel detectedModel = AirPodsModel.Unknown;
                        
                        switch (modelId)
                        {
                            case 0x02: detectedModel = AirPodsModel.AirPods1; break;
                            case 0x0F: detectedModel = AirPodsModel.AirPods2; break;
                            case 0x13: detectedModel = AirPodsModel.AirPods3; break;
                            case 0x22: detectedModel = AirPodsModel.AirPods4; break;
                            case 0x0E: detectedModel = AirPodsModel.AirPodsPro1; break;
                            case 0x14: detectedModel = AirPodsModel.AirPodsPro2; break;
                            case 0x0A: detectedModel = AirPodsModel.AirPodsMax; break;
                            case 0x0B: detectedModel = AirPodsModel.PowerbeatsPro; break;
                            case 0x12: detectedModel = AirPodsModel.BeatsFitPro; break;
                            case 0x11: detectedModel = AirPodsModel.BeatsStudioBuds; break;
                            default: detectedModel = AirPodsModel.Unknown; break;
                        }

                        // AirPods typically broadcast type 0x07.
                        var rightBat = (data[12] & 0x0F) == 0x0F ? 15 : (data[12] & 0x0F) * 10;
                        var leftBat = ((data[12] & 0xF0) >> 4) == 0x0F ? 15 : ((data[12] & 0xF0) >> 4) * 10;
                        var caseBat = (data[13] & 0x0F) == 0x0F ? 15 : (data[13] & 0x0F) * 10;
                        
                        // For Max, the battery is sometimes broadcast in the right earbud position, sometimes left. We'll take the max valid one.
                        if (detectedModel == AirPodsModel.AirPodsMax)
                        {
                            leftBat = Math.Max(leftBat == 15 ? -1 : leftBat, rightBat == 15 ? -1 : rightBat);
                            if (leftBat == -1) leftBat = 15;
                            rightBat = 15;
                            caseBat = 15;
                        }
                        
                        bool inEar = ((data[11] & 0x08) != 0) || ((data[11] & 0x02) != 0); // Estimation

                        if ((DateTime.Now - _lastUpdate).TotalSeconds > 1.5)
                        {
                            _lastUpdate = DateTime.Now;
                            AirPodsUpdated?.Invoke(this, new AirPodsData 
                            {
                                MacAddress = args.BluetoothAddress,
                                Model = detectedModel,
                                LeftBattery = leftBat,
                                RightBattery = rightBat,
                                CaseBattery = caseBat,
                                InEar = inEar
                            });
                        }
                    }
                }
            }
        }
    }
}
