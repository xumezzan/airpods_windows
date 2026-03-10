using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MagicPodsClone.Services
{
    public class GattConnectionService
    {
        private BluetoothLEDevice? _device;
        private GattCharacteristic? _proximityCharacteristic;

        // AirPods have custom vendor-specific GATT services. 
        // These UUIDs are historically known for Apple's proximity sensors.
        // We'll use mocked common Apple Service UUIDs for the concept:
        private readonly Guid AirPodsServiceUuid = new Guid("0000aaaa-0000-1000-8000-00805f9b34fb"); // Example dummy UUID
        private readonly Guid AirPodsProximityCharacteristicUuid = new Guid("0000bbbb-0000-1000-8000-00805f9b34fb"); // Example dummy UUID

        public async Task<bool> ConnectAsync(ulong bluetoothAddress)
        {
            try
            {
                _device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
                if (_device == null) return false;

                var gattServices = await _device.GetGattServicesForUuidAsync(AirPodsServiceUuid);
                if (gattServices.Status == GattCommunicationStatus.Success && gattServices.Services.Count > 0)
                {
                    var service = gattServices.Services[0];
                    var characteristics = await service.GetCharacteristicsForUuidAsync(AirPodsProximityCharacteristicUuid);
                    
                    if (characteristics.Status == GattCommunicationStatus.Success && characteristics.Characteristics.Count > 0)
                    {
                        _proximityCharacteristic = characteristics.Characteristics[0];

                        // Subscribe to value changes
                        _proximityCharacteristic.ValueChanged += OnProximityValueChanged;
                        
                        var status = await _proximityCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        
                        return status == GattCommunicationStatus.Success;
                    }
                }
            }
            catch (Exception)
            {
                // In a production app: log the exception securely
                return false;
            }

            return false;
        }

        public void Disconnect()
        {
            if (_proximityCharacteristic != null)
            {
                _proximityCharacteristic.ValueChanged -= OnProximityValueChanged;
                _proximityCharacteristic = null;
            }

            _device?.Dispose();
            _device = null;
        }

        private void OnProximityValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // Parse the new value. Typically 0x00 means not in ear, 0x01 means in ear
            var reader = Windows.Storage.Streams.DataReader.FromBuffer(args.CharacteristicValue);
            byte[] data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);

            if (data.Length > 0)
            {
                bool inEar = data[0] == 0x01; // Simplified logic based on common BLE reversing
                
                // If it was just taken out of the ear, pause the media
                if (!inEar)
                {
                    MediaController.TogglePlayPause();
                }
            }
        }
    }
}
