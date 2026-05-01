using System;
using System.Linq;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Systems
{

public sealed class DispensaryEconomySystem
{
    private readonly IGameApi _gameApi;
    private readonly DispensarySaveData _saveData;
    private readonly DispensaryEmployeeManager _employeeManager;

    public DispensaryEconomySystem(IGameApi gameApi, DispensarySaveData saveData, DispensaryEmployeeManager employeeManager)
    {
        _gameApi = gameApi;
        _saveData = saveData;
        _employeeManager = employeeManager;

        _gameApi.Events.OnDailyTick += SimulateDailyEconomy;
    }

    private void SimulateDailyEconomy()
    {
        foreach (var dispensary in _saveData.OwnedDispensaries.Values)
        {
            decimal revenue = 0m;
            var speed = _employeeManager.GetEffectiveSalesMultiplier(dispensary);

            foreach (var listing in dispensary.Inventory.Listings.ToList())
            {
                if (listing.Quantity <= 0)
                {
                    dispensary.Inventory.Listings.Remove(listing);
                    continue;
                }

                var gradeBonus = listing.Grade switch
                {
                    ProductGrade.Low => 0.8f,
                    ProductGrade.Mid => 1.0f,
                    ProductGrade.High => 1.25f,
                    ProductGrade.Exotic => 1.5f,
                    _ => 1.0f,
                };

                var demand = dispensary.LocationDemandModifier * listing.DemandModifier * gradeBonus * speed;
                var unitsSold = Math.Clamp((int)MathF.Round(demand * 4f), 0, listing.Quantity);
                listing.Quantity -= unitsSold;
                revenue += unitsSold * listing.SalePrice;
            }

            var wages = _employeeManager.CalculateDailyWages(dispensary);
            var report = new DispensaryDailyReport
            {
                DayUtc = DateTime.UtcNow.Date,
                Revenue = revenue,
                OperatingCosts = dispensary.OperatingCostPerDay,
                Wages = wages,
            };

            dispensary.Reports.Add(report);
            if (dispensary.Reports.Count > 30)
            {
                dispensary.Reports.RemoveAt(0);
            }

            _gameApi.Player.Cash += report.Profit;
            _gameApi.Log.Info($"Dispensary daily tick completed: {dispensary.Id}, revenue={report.Revenue}, profit={report.Profit}");
        }
    }
}

}
