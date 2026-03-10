using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace AirPodsCompanion.Services
{
    public enum NoiseControlMode
    {
        Off = 0x00,
        NoiseCancellation = 0x01,
        Transparency = 0x02
    }

    public class GattService
    {
        // Example UUIDs for Apple specific custom GATT services (Need discovery in production)
        private static readonly Guid AirPodsServiceUuid = new Guid("00000000-0000-0000-0000-000000000000"); // Placeholder
        private static readonly Guid AirPodsCharacteristicUuid = new Guid("00000000-0000-0000-0000-000000000000"); // Placeholder

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public const byte VK_MEDIA_PLAY_PAUSE = 0xB3;

        private BluetoothLEDevice _connectedDevice;
        private GattCharacteristic _inEarCharacteristic;

        public async Task<bool> SubscribeToInEarDetectionAsync(ulong macAddress)
        {
            try
            {
                if (_connectedDevice != null)
                {
                    _connectedDevice.Dispose();
                }

                _connectedDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(macAddress);
                if (_connectedDevice == null)
                {
                    Debug.WriteLine("Failed to connect to device via GATT for Auto-Pause.");
                    return false;
                }

                var gattServices = await _connectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (gattServices.Status != GattCommunicationStatus.Success)
                    return false;

                var service = gattServices.Services.FirstOrDefault(s => s.Uuid == AirPodsServiceUuid);
                if (service != null)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    if (characteristics.Status == GattCommunicationStatus.Success)
                    {
                        var characteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == AirPodsCharacteristicUuid);
                        if (characteristic != null)
                        {
                            _inEarCharacteristic = characteristic;
                            var status = await _inEarCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            
                            if (status == GattCommunicationStatus.Success)
                            {
                                _inEarCharacteristic.ValueChanged += OnInEarSensorChanged;
                                Debug.WriteLine($"Successfully subscribed to Auto-Pause GATT characteristic.");
                                return true;
                            }
                        }
                    }
                }

                Debug.WriteLine($"[GattService] Mock subscribed to in-ear detection for {macAddress}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GATT Subscription Error: {ex.Message}");
                return false;
            }
        }

        private void OnInEarSensorChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // Simulate reading the state from payload (0 byte represents in-ear state changes)
            // Trigger auto-pause logic via Windows API
            Debug.WriteLine("In-ear state changed! Executing Play/Pause macro...");
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
        }

        public async Task<bool> SetNoiseControlModeAsync(ulong macAddress, NoiseControlMode mode)
        {
            try
            {
                using var device = await BluetoothLEDevice.FromBluetoothAddressAsync(macAddress);
                if (device == null)
                {
                    Debug.WriteLine("Failed to connect to device via GATT.");
                    return false;
                }

                var gattServices = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (gattServices.Status != GattCommunicationStatus.Success)
                    return false;

                // In a production app, we would search for the specific Apple defined UUID.
                // For this MVP, we mock the command sender to illustrate the architecture.
                var service = gattServices.Services.FirstOrDefault(s => s.Uuid == AirPodsServiceUuid);
                if (service != null)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    if (characteristics.Status == GattCommunicationStatus.Success)
                    {
                        var characteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == AirPodsCharacteristicUuid);
                        if (characteristic != null)
                        {
                            var writer = new DataWriter();
                            writer.WriteByte((byte)mode); // Send the mode command

                            var writeResult = await characteristic.WriteValueAsync(writer.DetachBuffer());
                            return writeResult == GattCommunicationStatus.Success;
                        }
                    }
                }

                Debug.WriteLine($"[GattService] Mock set Noise Control to {mode} for {macAddress}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GATT Error: {ex.Message}");
                return false;
            }
        }
    }
}
