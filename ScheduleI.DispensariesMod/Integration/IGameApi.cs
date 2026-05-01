using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration
{
    public interface IGameApi
    {
        IGameEvents Events { get; }
        IProgressionApi Progression { get; }
        INotificationApi Notifications { get; }
        IPlayerApi Player { get; }
        IRealEstateApi RealEstate { get; }
        IInventoryApi Inventory { get; }
        ISaveSystemApi SaveSystem { get; }
        IUiApi Ui { get; }
        ILogApi Log { get; }
        IIntegrationDiagnostics Diagnostics { get; }
    }

    public interface IGameEvents { event Action OnDailyTick; }
    public interface IProgressionApi
    {
        bool TryGetFlag(string key, out bool value);
        bool TryGetMayorWeedLegalizationFlag(out string resolvedFlag, out bool value);
    }
    public interface INotificationApi { void Show(string message); }
    public interface IPlayerApi { bool IsAvailable { get; } decimal Cash { get; set; } }
    public interface IInventoryApi
    {
        IList<PlayerProductStack> GetCannabisProducts();
        bool TryRemove(string productId, int quantity);
        bool TryAddDebugProduct(string productId, string productName, ProductGrade grade, int quantity, decimal basePrice);
    }
    public interface IUiApi
    {
        void RegisterMenu(string menuId, Action openAction);
        void OpenDispensaryManagement(string dispensaryId);
        void OpenDispensaryListings(string dispensaryId);
        void OpenDispensaryHiring(string dispensaryId);
        void OpenDispensaryReports(string dispensaryId);
    }
    public interface ISaveSystemApi
    {
        bool IsAvailable { get; }
        void Register(string key, Func<string> save, Action<string> load);
    }
    public interface IRealEstateApi
    {
        bool IsAvailable { get; }
        void RegisterDynamicListingProvider(string categoryId, Func<IList<RealEstateListing>> provider);
        event Func<string, bool> OnPurchaseRequested;
    }
    public interface ILogApi
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception ex);
    }
    public interface IIntegrationDiagnostics
    {
        IntegrationStatus GetStatus();
        void DumpStatus();
    }

    public class IntegrationStatus
    {
        public bool LegalizationFlagLookupFound { get; set; }
        public bool RealEstateHookFound { get; set; }
        public bool UiHookFound { get; set; }
        public bool SaveHookFound { get; set; }
        public bool DailyTickFound { get; set; }
    }

    public class RealEstateListing
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }

        public RealEstateListing() { Id = Name = Location = Category = string.Empty; }
        public RealEstateListing(string id, string name, string location, decimal price, string category) { Id = id; Name = name; Location = location; Price = price; Category = category; }
    }

    public class PlayerProductStack
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public ProductGrade Grade { get; set; }
        public int Quantity { get; set; }
        public decimal BaseStreetPrice { get; set; }

        public PlayerProductStack() { ProductId = ProductName = string.Empty; }
    }

    public enum ProductGrade { Low, Mid, High, Exotic }
    public enum PurchaseFailReason { None, LegalizationLocked, ListingNotFound, AlreadyOwned, InsufficientFunds, PlayerCashApiMissing, SaveApiMissing }
}
