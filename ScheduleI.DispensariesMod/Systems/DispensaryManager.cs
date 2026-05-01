using System.Collections.Generic;
using System.Linq;
using ScheduleI.DispensariesMod.Data;
using ScheduleI.DispensariesMod.Integration;

namespace ScheduleI.DispensariesMod.Systems
{
    public sealed class DispensaryManager
    {
        private readonly IGameApi _gameApi;
        private readonly DispensarySaveData _saveData;
        private readonly Dictionary<string, DispensaryPropertyTemplate> _templates = new Dictionary<string, DispensaryPropertyTemplate>();

        public DispensaryManager(IGameApi gameApi, DispensarySaveData saveData, DispensaryEmployeeManager employeeManager, DispensaryEconomySystem economySystem)
        {
            _gameApi = gameApi;
            _saveData = saveData;
        }

        public void RegisterTemplate(DispensaryPropertyTemplate template) { _templates[template.Id] = template; }

        public IList<DispensaryPropertyTemplate> GetAvailableTemplates()
        {
            return _templates.Values.Where(t => !_saveData.OwnedDispensaries.ContainsKey(t.Id)).ToList();
        }

        public bool TryPurchaseDispensary(string templateId, bool cannabisLegalized, out PurchaseFailReason failReason)
        {
            failReason = PurchaseFailReason.None;
            if (!cannabisLegalized) { failReason = PurchaseFailReason.LegalizationLocked; return false; }
            DispensaryPropertyTemplate template;
            if (!_templates.TryGetValue(templateId, out template)) { failReason = PurchaseFailReason.ListingNotFound; return false; }
            if (_saveData.OwnedDispensaries.ContainsKey(template.Id)) { failReason = PurchaseFailReason.AlreadyOwned; return false; }
            if (!_gameApi.Player.IsAvailable) { failReason = PurchaseFailReason.PlayerCashApiMissing; return false; }
            if (_gameApi.Player.Cash < template.PurchasePrice) { failReason = PurchaseFailReason.InsufficientFunds; return false; }
            if (!_gameApi.SaveSystem.IsAvailable) { failReason = PurchaseFailReason.SaveApiMissing; return false; }

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
            _gameApi.Notifications.Show("Purchased dispensary: " + template.Name);
            return true;
        }

        public bool TryRename(string dispensaryId, string newName)
        {
            DispensaryProperty dispensary;
            if (!_saveData.OwnedDispensaries.TryGetValue(dispensaryId, out dispensary)) return false;
            dispensary.Name = newName;
            return true;
        }

        public bool TryListProduct(string dispensaryId, PlayerProductStack stack, int quantity, decimal salePrice, float demandModifier)
        {
            DispensaryProperty dispensary;
            if (!_saveData.OwnedDispensaries.TryGetValue(dispensaryId, out dispensary)) return false;
            if (dispensary.Inventory.Listings.Count >= dispensary.DisplaySlots || !_gameApi.Inventory.TryRemove(stack.ProductId, quantity)) return false;
            dispensary.Inventory.Listings.Add(new DispensaryListing { ProductId = stack.ProductId, ProductName = stack.ProductName, Grade = stack.Grade, Quantity = quantity, SalePrice = salePrice, DemandModifier = demandModifier });
            return true;
        }
    }
}
