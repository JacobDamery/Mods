using System;
using System.Collections.Generic;
using System.Text;
using ScheduleI.DispensariesMod.Data;

namespace ScheduleI.DispensariesMod.Systems
{
    public class DispensarySaveData
    {
        public int Version { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public bool CannabisLegalized { get; set; }
        public bool DebugLegalizationForced { get; set; }
        public Dictionary<string, DispensaryProperty> OwnedDispensaries { get; set; }
        public Dictionary<string, DispensaryEmployee> EmployeePool { get; set; }

        public DispensarySaveData()
        {
            Version = 1;
            LastUpdatedUtc = DateTime.UtcNow;
            OwnedDispensaries = new Dictionary<string, DispensaryProperty>();
            EmployeePool = new Dictionary<string, DispensaryEmployee>();
        }

        public string ToJson()
        {
            LastUpdatedUtc = DateTime.UtcNow;
            // Compatible fallback serialization (line-based key=value), avoids unavailable System.Text.Json.
            var sb = new StringBuilder();
            sb.AppendLine("Version=" + Version);
            sb.AppendLine("LastUpdatedUtc=" + LastUpdatedUtc.ToString("o"));
            sb.AppendLine("CannabisLegalized=" + CannabisLegalized);
            sb.AppendLine("DebugLegalizationForced=" + DebugLegalizationForced);
            sb.AppendLine("OwnedIds=" + string.Join(",", OwnedDispensaries.Keys));
            return sb.ToString();
        }

        public void LoadFromJson(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            var lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var idx = line.IndexOf('=');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx);
                var value = line.Substring(idx + 1);
                if (key == "Version") { int i; if (int.TryParse(value, out i)) Version = i; }
                else if (key == "LastUpdatedUtc") { DateTime dt; if (DateTime.TryParse(value, out dt)) LastUpdatedUtc = dt; }
                else if (key == "CannabisLegalized") { bool b; if (bool.TryParse(value, out b)) CannabisLegalized = b; }
                else if (key == "DebugLegalizationForced") { bool b2; if (bool.TryParse(value, out b2)) DebugLegalizationForced = b2; }
            }
        }
    }
}
