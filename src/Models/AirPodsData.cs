using System;

namespace AirPodsCompanion.Models
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
        public bool InEar { get; set; } = false;
        public bool IsCharging { get; set; } = false;
        public bool LidOpen { get; set; } = false;
        public short RSSI { get; set; } = -100;
        public DateTime LastSeen { get; set; } = DateTime.MinValue;

        public void UpdateBattery(int left, int right, int c)
        {
            if (left != 15) LeftBattery = left;
            if (right != 15) RightBattery = right;
            if (c != 15) CaseBattery = c;
        }

        public string GetBatteryString()
        {
            string lStr = LeftBattery == 15 ? "--" : $"{LeftBattery}%";
            string rStr = RightBattery == 15 ? "--" : $"{RightBattery}%";
            string cStr = CaseBattery == 15 ? "--" : $"{CaseBattery}%";
            return $"L: {lStr} | R: {rStr} | Case: {cStr}";
        }
    }
}
