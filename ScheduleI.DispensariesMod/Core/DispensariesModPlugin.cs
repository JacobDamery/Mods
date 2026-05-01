using System;
using System.Collections.Generic;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;
using ScheduleI.DispensariesMod.Systems;
using ScheduleI.DispensariesMod.UI;

namespace ScheduleI.DispensariesMod.Core;

/// <summary>
/// Entry point for the dispensary gameplay expansion. Hook this into your mod loader initialization.
/// </summary>
public sealed class DispensariesModPlugin
{
    private readonly IGameApi _gameApi;

    public DispensaryManager DispensaryManager { get; }
    public DispensaryEmployeeManager EmployeeManager { get; }
    public DispensaryEconomySystem EconomySystem { get; }
    public DispensarySaveData SaveData { get; }
    public DispensaryUiController UiController { get; }

    public DispensariesModPlugin(IGameApi gameApi)
    {
        _gameApi = gameApi;
        SaveData = new DispensarySaveData();

        EmployeeManager = new DispensaryEmployeeManager(_gameApi, SaveData);
        EconomySystem = new DispensaryEconomySystem(_gameApi, SaveData, EmployeeManager);
        DispensaryManager = new DispensaryManager(_gameApi, SaveData, EmployeeManager, EconomySystem);
        UiController = new DispensaryUiController(_gameApi, DispensaryManager, EmployeeManager, SaveData);
    }

    public void Initialize()
    {
        RegisterDispensaryCatalog();
        WireProgressionUnlock();
        WireRealEstateIntegration();
        WireSaveLoad();
    }

    private void RegisterDispensaryCatalog()
    {
        DispensaryManager.RegisterTemplate(new DispensaryPropertyTemplate("Downtown Leaf", "Downtown", 250000m, 4500m, 350, 5, 12, 1.25f));
        DispensaryManager.RegisterTemplate(new DispensaryPropertyTemplate("Harbor Green", "Harbor District", 420000m, 6400m, 550, 8, 18, 1.45f));
        DispensaryManager.RegisterTemplate(new DispensaryPropertyTemplate("Midtown Collective", "Midtown", 325000m, 5200m, 420, 6, 14, 1.32f));
    }

    private void WireProgressionUnlock()
    {
        _gameApi.Events.OnDailyTick += () =>
        {
            if (SaveData.CannabisLegalized)
            {
                return;
            }

            if (_gameApi.Progression.TryGetFlag("MayorPubliclySmokingWeed", out var mayorSmokingFlag) && mayorSmokingFlag)
            {
                SaveData.CannabisLegalized = true;
                _gameApi.Notifications.Show("Cannabis has been legalized. Dispensaries are now available to purchase.");
            }
        };
    }

    private void WireRealEstateIntegration()
    {
        _gameApi.RealEstate.RegisterDynamicListingProvider("dispensaries", () =>
        {
            if (!SaveData.CannabisLegalized)
            {
                return Array.Empty<RealEstateListing>();
            }

            var listings = new List<RealEstateListing>();
            foreach (var template in DispensaryManager.GetAvailableTemplates())
            {
                listings.Add(new RealEstateListing(template.Id, template.Name, template.Location, template.PurchasePrice, "Dispensary"));
            }

            return listings;
        });

        _gameApi.RealEstate.OnPurchaseRequested += listingId =>
        {
            if (!SaveData.CannabisLegalized)
            {
                _gameApi.Notifications.Show("Dispensaries are not legal yet.");
                return false;
            }

            return DispensaryManager.TryPurchaseDispensary(listingId);
        };
    }

    private void WireSaveLoad()
    {
        _gameApi.SaveSystem.Register(
            "dispensaries_mod_state",
            () => SaveData.ToJson(),
            json => SaveData.LoadFromJson(json));
    }
}
