using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Systems;

public sealed class DispensaryManager
{
    private readonly IGameApi _gameApi;
    private readonly DispensarySaveData _saveData;
    private readonly Dictionary<string, DispensaryPropertyTemplate> _templates = new();

    public DispensaryManager(IGameApi gameApi, DispensarySaveData saveData, DispensaryEmployeeManager employeeManager, DispensaryEconomySystem economySystem)
    {
        _gameApi = gameApi;
        _saveData = saveData;
    }

    public void RegisterTemplate(DispensaryPropertyTemplate template) => _templates[template.Id] = template;

    public IReadOnlyList<DispensaryPropertyTemplate> GetAvailableTemplates()
        => _templates.Values.Where(t => !_saveData.OwnedDispensaries.ContainsKey(t.Id)).ToList();

    public bool TryPurchaseDispensary(string templateId)
    {
        if (!_templates.TryGetValue(templateId, out var template))
        {
            return false;
        }

        if (_saveData.OwnedDispensaries.ContainsKey(template.Id) || _gameApi.Player.Cash < template.PurchasePrice)
        {
            return false;
        }

        _gameApi.Player.Cash -= template.PurchasePrice;
        _saveData.OwnedDispensaries[template.Id] = new DispensaryProperty
        {
            Id = template.Id,
            Name = template.Name,
            Location = template.Location,
            PurchasePrice = template.PurchasePrice,
            OperatingCostPerDay = template.OperatingCostPerDay,
            StorageCapacity = template.StorageCapacity,
            EmployeeSlots = template.EmployeeSlots,
            DisplaySlots = template.DisplaySlots,
            LocationDemandModifier = template.LocationDemandModifier,
        };

        _gameApi.Notifications.Show($"Purchased dispensary: {template.Name}");
        return true;
    }

    public bool TryRename(string dispensaryId, string newName)
    {
        if (!_saveData.OwnedDispensaries.TryGetValue(dispensaryId, out var dispensary))
        {
            return false;
        }

        dispensary.Name = newName;
        return true;
    }

    public bool TryListProduct(string dispensaryId, PlayerProductStack stack, int quantity, decimal salePrice, float demandModifier)
    {
        if (!_saveData.OwnedDispensaries.TryGetValue(dispensaryId, out var dispensary))
        {
            return false;
        }

        if (dispensary.Inventory.Listings.Count >= dispensary.DisplaySlots || !_gameApi.Inventory.TryRemove(stack.ProductId, quantity))
        {
            return false;
        }

        dispensary.Inventory.Listings.Add(new DispensaryListing
        {
            ProductId = stack.ProductId,
            ProductName = stack.ProductName,
            Grade = stack.Grade,
            Quantity = quantity,
            SalePrice = salePrice,
            DemandModifier = demandModifier,
        });

        return true;
    }
}
