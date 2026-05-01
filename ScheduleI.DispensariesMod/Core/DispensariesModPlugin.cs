using System;
using System.Collections.Generic;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;
using ScheduleI.DispensariesMod.Systems;
using ScheduleI.DispensariesMod.UI;

namespace ScheduleI.DispensariesMod.Core;

public sealed class DispensariesModPlugin
{
    private readonly IGameApi _gameApi;
    private readonly bool _debugMode;

    public DispensaryManager DispensaryManager { get; }
    public DispensaryEmployeeManager EmployeeManager { get; }
    public DispensaryEconomySystem EconomySystem { get; }
    public DispensarySaveData SaveData { get; }
    public DispensaryUiController UiController { get; }

    public DispensariesModPlugin(IGameApi gameApi, bool debugMode = false)
    {
        _gameApi = gameApi;
        _debugMode = debugMode;
        SaveData = new DispensarySaveData();

        EmployeeManager = new DispensaryEmployeeManager(_gameApi, SaveData);
        EconomySystem = new DispensaryEconomySystem(_gameApi, SaveData, EmployeeManager);
        DispensaryManager = new DispensaryManager(_gameApi, SaveData, EmployeeManager, EconomySystem);
        UiController = new DispensaryUiController(_gameApi, DispensaryManager, EmployeeManager, SaveData);
    }

    public void Initialize()
    {
        _gameApi.Log.Info("DispensariesMod: initializing");
        _gameApi.Diagnostics.DumpStatus();

        RegisterDispensaryCatalog();
        WireProgressionUnlock();
        WireRealEstateIntegration();
        WireSaveLoad();

        if (_debugMode)
        {
            ApplyDebugShortcuts();
        }
    }

    private void RegisterDispensaryCatalog()
    {
        // vertical slice: exactly one guaranteed test dispensary
        DispensaryManager.RegisterTemplate(new DispensaryPropertyTemplate("Test Leaf", "Downtown", 150000m, 3500m, 200, 3, 8, 1.1f));
    }

    private void WireProgressionUnlock()
    {
        _gameApi.Events.OnDailyTick += () =>
        {
            if (SaveData.CannabisLegalized) return;

            if (_gameApi.Progression.TryGetMayorWeedLegalizationFlag(out var flagName, out var unlocked) && unlocked)
            {
                SaveData.CannabisLegalized = true;
                _gameApi.Notifications.Show("Cannabis has been legalized. Dispensaries are now available to purchase.");
                _gameApi.Log.Info($"DispensariesMod: legalization unlocked via flag {flagName}");
                return;
            }

            _gameApi.Log.Warn("DispensariesMod: legalization remains locked (no matching flag found). ");
        };
    }

    private void WireRealEstateIntegration()
    {
        _gameApi.RealEstate.RegisterDynamicListingProvider("dispensaries", () =>
        {
            if (!SaveData.CannabisLegalized) return Array.Empty<RealEstateListing>();

            var listings = new List<RealEstateListing>();
            foreach (var template in DispensaryManager.GetAvailableTemplates())
            {
                listings.Add(new RealEstateListing(template.Id, template.Name, template.Location, template.PurchasePrice, "Dispensary"));
            }

            _gameApi.Log.Info($"DispensariesMod: injected {listings.Count} dispensary listing(s)");
            return listings;
        });

        _gameApi.RealEstate.OnPurchaseRequested += listingId =>
        {
            var purchased = DispensaryManager.TryPurchaseDispensary(listingId, SaveData.CannabisLegalized, out var reason);
            if (!purchased)
            {
                _gameApi.Log.Warn($"DispensariesMod: purchase failed for {listingId}. Reason={reason}");
            }
            else
            {
                _gameApi.Log.Info($"DispensariesMod: purchase succeeded for {listingId}");
                _gameApi.Ui.OpenDispensaryManagement(listingId);
            }

            return purchased;
        };
    }

    private void WireSaveLoad()
    {
        _gameApi.SaveSystem.Register(
            "dispensaries_mod_state",
            () => SaveData.ToJson(),
            json => SaveData.LoadFromJson(json));
    }

    private void ApplyDebugShortcuts()
    {
        SaveData.CannabisLegalized = true;
        if (_gameApi.Player.IsAvailable)
        {
            _gameApi.Player.Cash += 1_000_000m;
        }

        _gameApi.Inventory.TryAddDebugProduct("debug_og", "Debug OG", ProductGrade.High, 30, 85m);
        _gameApi.Ui.RegisterMenu("dispensary.debug.open", () => _gameApi.Ui.OpenDispensaryManagement("test_leaf"));
        _gameApi.Log.Info("DispensariesMod: debug shortcuts applied (legalization forced, cash granted, product added, direct panel open enabled)");
    }
}
