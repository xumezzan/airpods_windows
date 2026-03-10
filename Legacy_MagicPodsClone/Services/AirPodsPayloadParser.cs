using System;
using System.Diagnostics;
using MagicPodsClone.Models;

namespace MagicPodsClone.Services
{
    public static class AirPodsPayloadParser
    {
        // Parses the 0x07 Manufacturer specific data (Proximity Pairing)
        public static bool TryParse(byte[] data, ulong macAddress, short rssi, out AirPodsState? state)
        {
            state = null;
            if (data == null || data.Length < 25)
            {
                // Not a valid or complete AirPods beacon
                return false;
            }

            // Expected format check (Data[0] is often the length, Data[1] is model ID)
            // But from Advertisement.ManufacturerData, CompanyID is already stripped.
            // Payload usually starts with Type 0x07
            
            // Typical offsets:
            // Byte 12 contains Right & Left battery level (0-10 or 15 for disconnected)
            var batLeft = (data[12] & 0xF0) >> 4;
            var batRight = data[12] & 0x0F;
            
            // Byte 13 contains case battery (0-10 or 15) and charge statuses
            var batCase = data[13] & 0x0F;

            // Lid open status is often an inverted bit or specific bit mask, 
            // commonly bit 6 in Byte 11 or similar. We'll use a simplistic generic approach:
            var flip = (data[11] & 0x02) != 0; 

            // In ear status 
            bool inEarLeft = (data[6] & 0x08) != 0;
            bool inEarRight = (data[6] & 0x04) != 0;

            state = new AirPodsState
            {
                MacAddress = macAddress,
                Rssi = rssi,
                LastSeen = DateTime.Now,
                
                LeftBattery = batLeft == 15 ? -1 : batLeft * 10,
                RightBattery = batRight == 15 ? -1 : batRight * 10,
                CaseBattery = batCase == 15 ? -1 : batCase * 10,

                IsLidOpen = flip,
                IsLeftInEar = inEarLeft,
                IsRightInEar = inEarRight
            };

            return true;
        }
    }
}
