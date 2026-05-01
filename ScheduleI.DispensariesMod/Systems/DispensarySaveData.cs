using System;
using System.Collections.Generic;
using System.Text.Json;
using ScheduleI.DispensariesMod.Data;

namespace ScheduleI.DispensariesMod.Systems;

public sealed class DispensarySaveData
{
    public int Version { get; set; } = 1;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public bool CannabisLegalized { get; set; }
    public bool DebugLegalizationForced { get; set; }
    public Dictionary<string, DispensaryProperty> OwnedDispensaries { get; set; } = new();
    public Dictionary<string, DispensaryEmployee> EmployeePool { get; set; } = new();

    public string ToJson()
    {
        LastUpdatedUtc = DateTime.UtcNow;
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
    }

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;

        var loaded = JsonSerializer.Deserialize<DispensarySaveData>(json);
        if (loaded is null) return;

        Version = loaded.Version;
        LastUpdatedUtc = loaded.LastUpdatedUtc;
        CannabisLegalized = loaded.CannabisLegalized;
        DebugLegalizationForced = loaded.DebugLegalizationForced;
        OwnedDispensaries = loaded.OwnedDispensaries ?? new();
        EmployeePool = loaded.EmployeePool ?? new();
    }
}
