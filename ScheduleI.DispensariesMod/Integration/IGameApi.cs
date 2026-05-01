using System;
using System.Collections.Generic;

namespace ScheduleI.DispensariesMod.Integration;

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
    IReadOnlyList<PlayerProductStack> GetCannabisProducts();
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
    void RegisterDynamicListingProvider(string categoryId, Func<IReadOnlyList<RealEstateListing>> provider);
    event Func<string, bool> OnPurchaseRequested;
}
public interface ILogApi
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
}
public interface IIntegrationDiagnostics
{
    IntegrationStatus GetStatus();
    void DumpStatus();
}

public sealed record IntegrationStatus(
    bool LegalizationFlagLookupFound,
    bool RealEstateHookFound,
    bool UiHookFound,
    bool SaveHookFound,
    bool DailyTickFound);

public readonly record struct RealEstateListing(string Id, string Name, string Location, decimal Price, string Category);
public readonly record struct PlayerProductStack(string ProductId, string ProductName, ProductGrade Grade, int Quantity, decimal BaseStreetPrice);
public enum ProductGrade { Low, Mid, High, Exotic }
public enum PurchaseFailReason { None, LegalizationLocked, ListingNotFound, AlreadyOwned, InsufficientFunds, PlayerCashApiMissing, SaveApiMissing }
