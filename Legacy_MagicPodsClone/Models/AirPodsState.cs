using System;

namespace MagicPodsClone.Models
{
    public class AirPodsState
    {
        public ulong MacAddress { get; set; }
        public bool IsLidOpen { get; set; }
        
        public int LeftBattery { get; set; }
        public int RightBattery { get; set; }
        public int CaseBattery { get; set; }

        public bool IsLeftInEar { get; set; }
        public bool IsRightInEar { get; set; }

        public bool IsLeftCharging { get; set; }
        public bool IsRightCharging { get; set; }
        public bool IsCaseCharging { get; set; }

        public int Rssi { get; set; }
        public DateTime LastSeen { get; set; }

        public bool IsDisconnected => (DateTime.Now - LastSeen).TotalSeconds > 5;
    }
}
