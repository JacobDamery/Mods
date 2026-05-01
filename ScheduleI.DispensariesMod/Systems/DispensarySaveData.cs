using System.Collections.Generic;
using System.Text.Json;
using ScheduleI.DispensariesMod.Data;

namespace ScheduleI.DispensariesMod.Systems;

public sealed class DispensarySaveData
{
    public bool CannabisLegalized { get; set; }
    public Dictionary<string, DispensaryProperty> OwnedDispensaries { get; set; } = new();
    public Dictionary<string, DispensaryEmployee> EmployeePool { get; set; } = new();

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });

    public void LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var loaded = JsonSerializer.Deserialize<DispensarySaveData>(json);
        if (loaded is null)
        {
            return;
        }

        CannabisLegalized = loaded.CannabisLegalized;
        OwnedDispensaries = loaded.OwnedDispensaries ?? new();
        EmployeePool = loaded.EmployeePool ?? new();
    }
}
